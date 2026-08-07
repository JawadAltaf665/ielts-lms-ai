using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.Tutor;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the gateway call for one AI Tutor "ask" turn (Phase 7 Sprint 4) — the first real caller of
/// Sprint 3's <see cref="IConversationContextService"/>. Mirrors <see cref="GrammarCoachingJob"/>'s
/// shape (enqueue → resolve provider/prompt → invoke → persist), plus appending the user/assistant
/// <see cref="ConversationMessage"/> pair the Grammar flow never needed.
/// </summary>
public sealed partial class AskTutorJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    IConversationRepository conversations,
    IConversationMessageRepository messages,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    IConversationContextService conversationContext,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<AskTutorJob> logger)
{
    private const int MaxOutputTokens = 1024;
    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables = new Dictionary<string, string>();

    public async Task ExecuteAsync(Guid tenantId, Guid operationId, Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var operation = await operations.GetByIdAsync(AiOperationId.From(operationId), cancellationToken).ConfigureAwait(false);
        if (operation is null)
        {
            OperationNotFound(logger, operationId);
            return;
        }

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
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.AiTutor, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            await FailAsync(operation, AiErrors.PromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var contextId = ConversationId.From(conversationId);
        var assembled = await conversationContext
            .AssembleAsync(tenantId, userId, contextId, prompt, NoSystemVariables, operation.InputText, cancellationToken)
            .ConfigureAwait(false);
        if (assembled.IsFailure)
        {
            await FailAsync(operation, assembled.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var transcript = ConversationTranscriptFormatter.Flatten(assembled.Value);
        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.AiTutor, prompt.PromptVersion, provider.Kind, assembled.Value.SystemPrompt,
                transcript, TutorAnswerSchema.Name, TutorAnswerSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);
        if (invocation.IsFailure)
        {
            await FailAsync(operation, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var result = invocation.Value;
        TutorAnswerOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TutorAnswerOutput>(result.OutputJson, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        var status = parsed is not null && !string.IsNullOrWhiteSpace(parsed.Answer)
            ? AiResponseStatus.Succeeded
            : AiResponseStatus.Invalid;

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

        if (status == AiResponseStatus.Invalid)
        {
            operation.MarkFailed(AiErrors.InputRejected.Code, "The AI response failed structured-output validation.", now);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var conversation = await conversations.GetByIdAsync(contextId, cancellationToken).ConfigureAwait(false);
        if (conversation is null)
        {
            await FailAsync(operation, AiErrors.ConversationNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        var nextSequence = await ConversationSequencing.NextSequenceAsync(messages, tenantId, contextId, cancellationToken)
            .ConfigureAwait(false);
        var userMessage = ConversationMessage.Create(
            ConversationMessageId.From(guids.NewGuid()), tenantId, contextId, ConversationMessageRole.User,
            operation.InputText, nextSequence, ConversationMessageTokenEstimator.Estimate(operation.InputText),
            aiResponseId: null, now).Value;
        var assistantMessage = ConversationMessage.Create(
            ConversationMessageId.From(guids.NewGuid()), tenantId, contextId, ConversationMessageRole.Assistant,
            parsed!.Answer, nextSequence + 1, ConversationMessageTokenEstimator.Estimate(parsed.Answer),
            aiResponse.Id, now).Value;

        messages.Add(userMessage);
        messages.Add(assistantMessage);
        conversation.RecordMessage(now);

        operation.MarkSucceeded(aiResponse.Id, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
