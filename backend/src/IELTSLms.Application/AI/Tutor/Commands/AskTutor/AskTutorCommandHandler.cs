using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Tutor.Commands.AskTutor;

public sealed class AskTutorCommandHandler(
    IConversationRepository conversations, IConversationMessageRepository messages, IAiOperationRepository operations,
    IUnitOfWork unitOfWork, IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<AskTutorCommand, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(AskTutorCommand request, CancellationToken cancellationToken)
    {
        var entitled = await entitlements.HasFeatureAsync(request.TenantId, AiFeatures.AiTutor, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var conversation = await conversations.GetByIdAsync(ConversationId.From(request.ConversationId), cancellationToken)
            .ConfigureAwait(false);
        if (conversation is null || conversation.TenantId != request.TenantId || conversation.UserId != request.UserId
            || conversation.Feature != AiFeatures.AiTutor)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.ConversationNotFound);
        }

        var now = clock.UtcNow;

        // Code-level scope gate (FR-AITUTOR-001 exception flow) runs before quota is touched or any
        // gateway call is made — an out-of-scope question never costs the learner a quota unit.
        if (!Tutor.TutorScopeGuard.IsInScope(request.Question))
        {
            return await AcceptWithoutGatewayCallAsync(request, conversation, now, cancellationToken).ConfigureAwait(false);
        }

        var quotaCheck = await quotas.CheckAsync(request.TenantId, AiFeatures.AiTutorQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas.ConsumeAsync(request.TenantId, AiFeatures.AiTutorQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTutor, request.Question, now);
        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var conversationId = request.ConversationId;
        var userId = request.UserId;
        backgroundJobs.Enqueue<AskTutorJob>(job => job.ExecuteAsync(tenantId, operationId, conversationId, userId, CancellationToken.None));

        return Result.Success(new AiOperationAcceptedResponse(operationId, "queued"));
    }

    /// <summary>Out-of-scope questions never reach the gateway, but the endpoint contract is still
    /// "202 + operation resource" either way (API Design Standards §14.2 consistency) — the operation is
    /// created already-succeeded and the redirect is appended as the assistant turn synchronously.</summary>
    private async Task<Result<AiOperationAcceptedResponse>> AcceptWithoutGatewayCallAsync(
        AskTutorCommand request, Conversation conversation, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.From(request.ConversationId);
        var nextSequence = await ConversationSequencing.NextSequenceAsync(messages, request.TenantId, conversationId, cancellationToken)
            .ConfigureAwait(false);

        var userMessage = ConversationMessage.Create(
            ConversationMessageId.From(guids.NewGuid()), request.TenantId, conversationId, ConversationMessageRole.User,
            request.Question, nextSequence, ConversationMessageTokenEstimator.Estimate(request.Question), aiResponseId: null, now).Value;
        var assistantMessage = ConversationMessage.Create(
            ConversationMessageId.From(guids.NewGuid()), request.TenantId, conversationId, ConversationMessageRole.Assistant,
            Tutor.TutorScopeGuard.RedirectMessage, nextSequence + 1,
            ConversationMessageTokenEstimator.Estimate(Tutor.TutorScopeGuard.RedirectMessage), aiResponseId: null, now).Value;

        messages.Add(userMessage);
        messages.Add(assistantMessage);
        conversation.RecordMessage(now);

        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTutor, request.Question, now);
        operation.MarkSucceeded(aiResponseId: null, now);
        operations.Add(operation);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new AiOperationAcceptedResponse(operation.Id.Value, "succeeded"));
    }
}
