using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.BandPredictor;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Speaking;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the Band Prediction Workflow for an already-enqueued <see cref="AiOperation"/> (Agentic AI
/// Architecture §13.6): gather history across all skills (read-only, cross-context — no new write
/// path) → Band Predictor (single AI Gateway call) → confidence check → present prediction. Unlike
/// Writing/Speaking's multi-step pipeline, this is one gateway call over a compact history summary, not
/// a per-submission re-evaluation. ponytail: gathers each skill's most recent rows via a per-user
/// in-memory list rather than a dedicated cross-skill read model/materialized view — acceptable while
/// per-learner history stays small; revisit if attempt volume grows large enough to matter.
/// </summary>
public sealed partial class BandPredictionJob(
    IAiOperationRepository operations, IBandPredictionRepository predictions, IAiProviderRepository providers,
    IAiPromptRepository prompts, IAiResponseRepository responses, IWritingSubmissionRepository writingSubmissions,
    IWritingEvaluationRepository writingEvaluations, ISpeakingAttemptRepository speakingAttempts,
    ISpeakingEvaluationRepository speakingEvaluations, IReadingAttemptRepository readingAttempts,
    IListeningAttemptRepository listeningAttempts, IGrammarAttemptRepository grammarAttempts,
    IVocabularyProgressRepository vocabularyProgress, IMockExamResultRepository mockExamResults, IUnitOfWork unitOfWork,
    IAiGateway gateway, ITenantContextAccessor tenantContextAccessor, IClock clock, IGuidProvider guids,
    ILogger<BandPredictionJob> logger)
{
    private const int MaxOutputTokens = 512;
    private const int MaxItemsPerSkill = 5;

    public async Task ExecuteAsync(Guid tenantId, Guid operationId, Guid bandPredictionId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var operation = await operations.GetByIdAsync(AiOperationId.From(operationId), cancellationToken).ConfigureAwait(false);
        var prediction = await predictions
            .GetByIdAsync(BandPredictionId.From(bandPredictionId), cancellationToken)
            .ConfigureAwait(false);
        if (operation is null || prediction is null)
        {
            OperationOrPredictionNotFound(logger, operationId, bandPredictionId);
            return;
        }

        // Idempotency guard: Hangfire's global automatic-retry policy may re-run this job after a
        // mid-flight crash. A terminal operation has already been fully persisted.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.BandPredictor, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            await FailAsync(operation, prediction, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var userId = prediction.UserId;
        var history = await GatherHistoryAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        var inputJson = JsonSerializer.Serialize(new BandPredictorInput(history), JsonSerializerOptions.Web);

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.BandPredictor, prompt.PromptVersion, provider.Kind, prompt.SystemTemplate,
                inputJson, BandPredictorSchema.Name, BandPredictorSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            await FailAsync(operation, prediction, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var invocationResult = invocation.Value;
        var output = TryDeserialize<BandPredictorOutput>(invocationResult.OutputJson);
        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(Domain.AI.BandPrediction), bandPredictionId, ComputeHash(inputJson), invocationResult.Model,
            invocationResult.PromptTokens, invocationResult.CompletionTokens, invocationResult.LatencyMs,
            invocationResult.CostMinorUnits, invocationResult.CostCurrency,
            output is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            output is not null ? invocationResult.OutputJson : null, invocationResult.ProviderResponseId, now);

        if (responseCreate.IsFailure)
        {
            await FailAsync(operation, prediction, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var response = responseCreate.Value;
        responses.Add(response);

        if (output is null)
        {
            await FailAsync(operation, prediction, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var completeResult = prediction.Complete(
            output.OverallBand, output.ReadingBand, output.ListeningBand, output.WritingBand, output.SpeakingBand,
            output.Confidence, response.Id, now);
        if (completeResult.IsFailure)
        {
            await FailAsync(operation, prediction, completeResult.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        operation.MarkSucceeded(response.Id, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<HistoryItem>> GatherHistoryAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var history = new List<HistoryItem>();

        var submissions = await writingSubmissions.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        foreach (var submission in submissions.Take(MaxItemsPerSkill))
        {
            decimal? band = null;
            if (submission.Status == WritingSubmissionStatus.Evaluated && submission.WritingEvaluationId is { } evaluationId)
            {
                var evaluation = await writingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false);
                band = evaluation?.EstimatedBand;
            }

            history.Add(new HistoryItem("Writing", submission.CreatedAtUtc, band, null, null));
        }

        var attempts = await speakingAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        foreach (var attempt in attempts.Take(MaxItemsPerSkill))
        {
            decimal? band = null;
            if (attempt.Status == SpeakingAttemptStatus.Evaluated && attempt.SpeakingEvaluationId is { } evaluationId)
            {
                var evaluation = await speakingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false);
                band = evaluation?.EstimatedBand;
            }

            history.Add(new HistoryItem("Speaking", attempt.CreatedAtUtc, band, null, null));
        }

        var reading = await readingAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(reading.Take(MaxItemsPerSkill).Select(a => new HistoryItem("Reading", a.CreatedAtUtc, a.Band, a.RawScore, a.MaxScore)));

        var listening = await listeningAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(listening.Take(MaxItemsPerSkill).Select(a => new HistoryItem("Listening", a.CreatedAtUtc, a.Band, a.RawScore, a.MaxScore)));

        var grammar = await grammarAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(grammar.Take(MaxItemsPerSkill).Select(a => new HistoryItem("Grammar", a.CreatedAtUtc, null, a.RawScore, a.MaxScore)));

        var vocabulary = await vocabularyProgress.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(
            vocabulary.Where(v => v.LastGrade is not null).OrderByDescending(v => v.UpdatedAtUtc).Take(MaxItemsPerSkill)
                .Select(v => new HistoryItem("Vocabulary", v.UpdatedAtUtc ?? v.CreatedAtUtc, null, v.LastGrade, 5)));

        var mockExams = await mockExamResults.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(mockExams.Take(MaxItemsPerSkill).Select(r => new HistoryItem("MockExam", r.CreatedAtUtc, r.OverallBand, null, null)));

        return history;
    }

    private async Task FailAsync(AiOperation operation, Domain.AI.BandPrediction prediction, Error error, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        operation.MarkFailed(error.Code, error.Message, now);
        prediction.Fail(error.Code, error.Message, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static T? TryDeserialize<T>(string? json) where T : class
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private sealed record HistoryItem(string Skill, DateTimeOffset AtUtc, decimal? Band, int? RawScore, int? MaxScore);

    private sealed record BandPredictorInput(IReadOnlyList<HistoryItem> RecentActivity);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} or band prediction {BandPredictionId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationOrPredictionNotFound(ILogger logger, Guid operationId, Guid bandPredictionId);
}
