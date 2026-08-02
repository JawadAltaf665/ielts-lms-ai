using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.Teacher;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// Executes the gateway call for one AI Teacher turn — either the <c>Explain</c> step's opening
/// content (<paramref name="isSessionStart"/> <see langword="true"/>, no learner message yet) or the
/// learner's response to whichever step is current (Phase 7 Sprint 4). Owns persisting the
/// conversation turn(s) and driving <see cref="LessonSession"/>'s step-state machine — the CQRS
/// layout's "domain service on LessonSession, not scattered across the handler" note means the actual
/// transition decision lives on the aggregate (<see cref="LessonSession.Advance"/>/
/// <see cref="LessonSession.RecordCheckAttempt"/>); this job only decides *which* of those two to call.
/// </summary>
public sealed partial class TeacherLessonStepJob(
    IAiOperationRepository operations,
    IAiProviderRepository providers,
    IAiPromptRepository prompts,
    IAiResponseRepository responses,
    ILessonSessionRepository lessonSessions,
    IConversationRepository conversations,
    IConversationMessageRepository messages,
    IUnitOfWork unitOfWork,
    IAiGateway gateway,
    IConversationContextService conversationContext,
    ITenantContextAccessor tenantContextAccessor,
    IClock clock,
    IGuidProvider guids,
    ILogger<TeacherLessonStepJob> logger)
{
    private const int MaxOutputTokens = 1024;
    private static readonly IReadOnlyDictionary<string, string> NoSystemVariables = new Dictionary<string, string>();

    public async Task ExecuteAsync(
        Guid tenantId, Guid operationId, Guid lessonSessionId, bool isSessionStart, CancellationToken cancellationToken = default)
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

        var session = await lessonSessions.GetByIdAsync(LessonSessionId.From(lessonSessionId), cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            await FailAsync(operation, AiErrors.LessonSessionNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Captured once, before any state-machine mutation below — the prompt/schema this turn uses is
        // always the step the learner is (or the Teacher is initiating) responding to.
        var stepAtCallTime = session.CurrentStep;
        if (stepAtCallTime == LessonStep.Completed)
        {
            await FailAsync(operation, AiErrors.LessonSessionCompleted, cancellationToken).ConfigureAwait(false);
            return;
        }

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var scope = LessonStepScopes.ForStep(stepAtCallTime);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAndScopeAsync(tenantId, AiFeatures.AiTeacher, scope, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            await FailAsync(operation, AiErrors.TeacherStepPromptNotConfigured, cancellationToken).ConfigureAwait(false);
            return;
        }

        var conversationId = session.ConversationId;
        var assembled = await conversationContext
            .AssembleAsync(tenantId, session.UserId, conversationId, prompt, NoSystemVariables, operation.InputText, cancellationToken)
            .ConfigureAwait(false);
        if (assembled.IsFailure)
        {
            await FailAsync(operation, assembled.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var transcript = ConversationTranscriptFormatter.Flatten(assembled.Value);
        var (schemaName, schemaJson) = TeacherStepSchemas.ForStep(stepAtCallTime);
        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.AiTeacher, prompt.PromptVersion, provider.Kind, assembled.Value.SystemPrompt,
                transcript, schemaName, schemaJson, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);
        if (invocation.IsFailure)
        {
            await FailAsync(operation, invocation.Error, cancellationToken).ConfigureAwait(false);
            return;
        }

        var result = invocation.Value;
        TeacherStepOutput? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<TeacherStepOutput>(result.OutputJson, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            parsed = null;
        }

        var status = parsed is not null && !string.IsNullOrWhiteSpace(parsed.Content)
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

        var conversation = await conversations.GetByIdAsync(conversationId, cancellationToken).ConfigureAwait(false);
        if (conversation is null)
        {
            await FailAsync(operation, AiErrors.ConversationNotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        var nextSequence = await ConversationSequencing.NextSequenceAsync(messages, tenantId, conversationId, cancellationToken)
            .ConfigureAwait(false);
        var assistantSequence = nextSequence;

        if (!isSessionStart)
        {
            var userMessage = ConversationMessage.Create(
                ConversationMessageId.From(guids.NewGuid()), tenantId, conversationId, ConversationMessageRole.User,
                operation.InputText, nextSequence, ConversationMessageTokenEstimator.Estimate(operation.InputText),
                aiResponseId: null, now).Value;
            messages.Add(userMessage);
            assistantSequence = nextSequence + 1;
        }

        var assistantMessage = ConversationMessage.Create(
            ConversationMessageId.From(guids.NewGuid()), tenantId, conversationId, ConversationMessageRole.Assistant,
            parsed!.Content, assistantSequence, ConversationMessageTokenEstimator.Estimate(parsed.Content), aiResponse.Id, now).Value;
        messages.Add(assistantMessage);
        conversation.RecordMessage(now);

        if (isSessionStart)
        {
            // Explain's opening content was just delivered; the session stays at Explain until the
            // learner responds — the Teacher initiated, it does not also advance on its own turn.
        }
        else if (stepAtCallTime == LessonStep.CheckUnderstanding)
        {
            session.RecordCheckAttempt(parsed.Passed ?? false, now);
        }
        else
        {
            session.Advance(now);
        }

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
