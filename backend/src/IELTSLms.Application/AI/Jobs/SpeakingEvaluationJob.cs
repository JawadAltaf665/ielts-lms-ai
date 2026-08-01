using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.SpeakingExaminer;
using IELTSLms.Application.Speaking;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the Speaking Evaluation Workflow for an already-enqueued <see cref="AiOperation"/>
/// (Agentic AI Architecture §13.2): transcribe the recorded audio via <see cref="ISpeechToTextProvider"/>,
/// then a single Speaking Examiner call scores the four criteria and composes feedback in one
/// structured response (same merged-simplification as <see cref="WritingEvaluationJob"/>'s Examiner
/// step) — no parallel sub-agents, unlike Writing's Grammar/Vocabulary pair. Enqueued via
/// <c>IBackgroundJobService.Enqueue&lt;SpeakingEvaluationJob&gt;</c> from
/// <see cref="Speaking.Commands.SubmitSpeakingAttempt.SubmitSpeakingAttemptCommandHandler"/>; not a
/// MediatR handler, same reason as <see cref="WritingEvaluationJob"/> (Hangfire needs a plain,
/// container-resolvable class).
/// </summary>
public sealed partial class SpeakingEvaluationJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    ISpeakingAttemptRepository attempts,
    ISpeakingTaskRepository tasks,
    ISpeakingEvaluationRepository evaluations,
    IFileAssetRepository files,
    IFileStorage fileStorage,
    ISpeechToTextProvider speechToText,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<SpeakingEvaluationJob> logger)
{
    private const int ExaminerMaxOutputTokens = 2048;

    /// <summary>Below this examiner-reported confidence, the evaluation is flagged for human review (HITL) rather than silently accepted.</summary>
    private const decimal LowConfidenceThreshold = 0.7m;

    public async Task ExecuteAsync(
        Guid tenantId, Guid operationId, Guid speakingAttemptId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var operation = await operations
            .GetByIdAsync(AiOperationId.From(operationId), cancellationToken)
            .ConfigureAwait(false);
        if (operation is null)
        {
            OperationNotFound(logger, operationId);
            return;
        }

        // Idempotency guard: Hangfire's global automatic-retry policy may re-run this job after a
        // mid-flight crash. A terminal operation has already been fully persisted — redoing it would
        // double-charge the quota's intent and could write duplicate AiResponse rows.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var attempt = await attempts
            .GetByIdAsync(SpeakingAttemptId.From(speakingAttemptId), cancellationToken)
            .ConfigureAwait(false);
        if (attempt is null)
        {
            AttemptNotFound(logger, speakingAttemptId);
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var examinerPrompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.SpeakingExaminer, cancellationToken).ConfigureAwait(false);

        if (provider is null || examinerPrompt is null)
        {
            await FailAsync(operation, attempt, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var task = await tasks.GetByIdAsync(attempt.SpeakingTaskId, cancellationToken).ConfigureAwait(false);
        if (task is null)
        {
            await FailAsync(operation, attempt, SpeakingErrors.TaskNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        var file = await files.GetByIdAsync(attempt.FileId, cancellationToken).ConfigureAwait(false);
        if (file is null)
        {
            await FailAsync(operation, attempt, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Transcribe (Speaking Evaluation Workflow, step 1). A dedicated STT port, not the AiGateway —
        // see ISpeechToTextProvider's remarks. Any provider failure fails the operation loudly (503),
        // never proceeds with a silent empty transcript.
        var transcriptionResult = await TranscribeAsync(file.StorageKey, file.ContentType, file.FileName, cancellationToken)
            .ConfigureAwait(false);
        if (transcriptionResult.IsFailure)
        {
            await FailAsync(operation, attempt, transcriptionResult.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var transcript = transcriptionResult.Value;
        var setTranscriptResult = attempt.SetTranscript(transcript.Text, now);
        if (setTranscriptResult.IsFailure)
        {
            await FailAsync(operation, attempt, setTranscriptResult.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Speaking Examiner Agent: scores the four criteria from the transcript, and (merged
        // simplification, same as Writing) composes the learner-facing feedback in the same call.
        var examinerInput = JsonSerializer.Serialize(
            new SpeakingExaminerInput(task.PromptText, task.Part.ToString(), transcript.Text), JsonSerializerOptions.Web);

        var examinerInvocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.SpeakingExaminer, examinerPrompt.PromptVersion, provider.Kind, examinerPrompt.SystemTemplate,
                examinerInput, SpeakingExaminerSchema.Name, SpeakingExaminerSchema.Json, ExaminerMaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (examinerInvocation.IsFailure)
        {
            await FailAsync(operation, attempt, examinerInvocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var examinerResult = examinerInvocation.Value;
        var examinerOutput = TryDeserialize<SpeakingExaminerOutput>(examinerResult.OutputJson);
        var examinerResponseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, examinerPrompt.Id, examinerPrompt.PromptVersion,
            nameof(AiOperation), operationId, ComputeHash(examinerInput), examinerResult.Model, examinerResult.PromptTokens,
            examinerResult.CompletionTokens, examinerResult.LatencyMs, examinerResult.CostMinorUnits, examinerResult.CostCurrency,
            examinerOutput is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            examinerOutput is not null ? examinerResult.OutputJson : null, examinerResult.ProviderResponseId, now);

        if (examinerResponseCreate.IsFailure)
        {
            await FailAsync(operation, attempt, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var examinerResponse = examinerResponseCreate.Value;
        responses.Add(examinerResponse);

        if (examinerOutput is null)
        {
            await FailAsync(operation, attempt, AiErrors.InputRejected, cancellationToken).ConfigureAwait(false);
            return;
        }

        var evaluationCreate = SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(guids.NewGuid()), tenantId, attempt.Id, examinerOutput.FluencyCoherence,
            examinerOutput.LexicalResource, examinerOutput.GrammaticalRangeAccuracy, examinerOutput.Pronunciation,
            examinerOutput.Confidence, examinerOutput.FeedbackText, LowConfidenceThreshold, examinerResponse.Id, provider.Id, now);

        if (evaluationCreate.IsFailure)
        {
            await FailAsync(operation, attempt, evaluationCreate.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var evaluation = evaluationCreate.Value;
        evaluations.Add(evaluation);
        attempt.AttachEvaluation(evaluation.Id, evaluation.EstimatedBand, now);
        operation.MarkSucceeded(examinerResponse.Id, now);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<SpeechToTextResult>> TranscribeAsync(
        string storageKey, string contentType, string fileName, CancellationToken cancellationToken)
    {
        await using var audio = await fileStorage.DownloadAsync(storageKey, cancellationToken).ConfigureAwait(false);
        return await speechToText
            .TranscribeAsync(new SpeechToTextRequest(audio, contentType, fileName), cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task FailAsync(AiOperation operation, SpeakingAttempt attempt, Error error, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        operation.MarkFailed(error.Code, error.Message, now);
        attempt.MarkEvaluationFailed(now);
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

    private sealed record SpeakingExaminerInput(string TaskPrompt, string Part, string Transcript);

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationNotFound(ILogger logger, Guid operationId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Speaking attempt {SpeakingAttemptId} not found; the enqueued job has nothing to execute.")]
    private static partial void AttemptNotFound(ILogger logger, Guid speakingAttemptId);
}
