using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.QuestionVariants;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the question-variant generation gateway call for an already-enqueued <see cref="AiOperation"/>
/// and, on success, writes the resulting <see cref="QuestionVariantDraft"/> (Sprint 8 scope item 3 —
/// "generation assists, humans approve": the draft is created <see cref="QuestionVariantStatus.Pending"/>,
/// never auto-published). The draft reuses the operation's own id as its id — the simplest way for a
/// client to go from the 202 response's operation id to the draft it eventually produces without a
/// second lookup endpoint (this sprint's endpoint table has none). Mirrors
/// <see cref="GrammarCoachingJob"/>'s enqueue/idempotency/tenant-context shape.
/// </summary>
public sealed partial class GenerateQuestionVariantsJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IQuestionVariantDraftRepository drafts,
    IReadingQuestionRepository readingQuestions,
    IListeningQuestionRepository listeningQuestions,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    IPromptCompositionService promptComposition,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<GenerateQuestionVariantsJob> logger)
{
    private const int MaxOutputTokens = 1024;
    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables =
        new Dictionary<string, string>();

    public async Task ExecuteAsync(
        Guid tenantId, Guid operationId, QuestionVariantSourceType sourceType, Guid sourceQuestionId, int difficultyDelta,
        CancellationToken cancellationToken = default)
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
        // mid-flight crash. A terminal operation has already been fully persisted.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var sourceQuestionType = await ResolveSourceQuestionTypeAsync(sourceType, sourceQuestionId, cancellationToken).ConfigureAwait(false);
        if (sourceQuestionType is null)
        {
            await FailAsync(operation, QuestionVariantErrors.SourceQuestionNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        var kind = sourceType == QuestionVariantSourceType.Reading
            ? AiFeatures.ReadingQuestionVariants
            : AiFeatures.ListeningQuestionVariants;

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null ? null : await prompts.GetActiveForFeatureAsync(tenantId, kind, cancellationToken).ConfigureAwait(false);

        if (provider is null || prompt is null)
        {
            await FailAsync(operation, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var composed = promptComposition.Compose(prompt, NoSystemVariables, operation.InputText);
        if (composed.IsFailure)
        {
            await FailAsync(operation, composed.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, kind, prompt.PromptVersion, provider.Kind, composed.Value.SystemPrompt,
                composed.Value.UserContent, QuestionVariantSchema.Name, QuestionVariantSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            await FailAsync(operation, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var result = invocation.Value;
        QuestionVariantOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<QuestionVariantOutput>(result.OutputJson, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        var status = parsed is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid;
        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(AiOperation), operationId, ComputeInputHash(operation.InputText), result.Model, result.PromptTokens,
            result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency, status,
            status == AiResponseStatus.Succeeded ? result.OutputJson : null, result.ProviderResponseId, now);

        if (responseCreate.IsFailure)
        {
            await FailAsync(operation, responseCreate.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var aiResponse = responseCreate.Value;
        responses.Add(aiResponse);

        if (parsed is null)
        {
            operation.MarkFailed(AiErrors.InputRejected.Code, "The AI response failed structured-output validation.", now);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var draftContent = JsonSerializer.Serialize(new QuestionVariantContent(sourceQuestionType, parsed.Prompt, parsed.AnswerKey));
        var draftCreate = QuestionVariantDraft.Create(
            QuestionVariantDraftId.From(operationId), tenantId, sourceType, sourceQuestionId, draftContent, difficultyDelta,
            aiResponse.Id, now);
        if (draftCreate.IsFailure)
        {
            operation.MarkFailed(draftCreate.Error.Code, draftCreate.Error.Message, now);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        drafts.Add(draftCreate.Value);
        operation.MarkSucceeded(aiResponse.Id, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string?> ResolveSourceQuestionTypeAsync(
        QuestionVariantSourceType sourceType, Guid sourceQuestionId, CancellationToken cancellationToken)
    {
        if (sourceType == QuestionVariantSourceType.Reading)
        {
            var question = await readingQuestions
                .GetByIdAsync(ReadingQuestionId.From(sourceQuestionId), cancellationToken).ConfigureAwait(false);
            return question?.Type.ToString();
        }

        var listeningQuestion = await listeningQuestions
            .GetByIdAsync(ListeningQuestionId.From(sourceQuestionId), cancellationToken).ConfigureAwait(false);
        return listeningQuestion?.Type.ToString();
    }

    private async Task FailAsync(AiOperation operation, Error error, CancellationToken cancellationToken)
    {
        operation.MarkFailed(error.Code, error.Message, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ComputeInputHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationNotFound(ILogger logger, Guid operationId);
}
