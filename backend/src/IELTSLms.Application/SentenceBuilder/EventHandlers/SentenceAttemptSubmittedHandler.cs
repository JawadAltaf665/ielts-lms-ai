using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Domain.SentenceBuilder.Events;
using MediatR;

namespace IELTSLms.Application.SentenceBuilder.EventHandlers;

/// <summary>
/// Reacts to a scored <see cref="SentenceAttemptSubmittedDomainEvent"/> by kicking off the AI
/// scaffolding enrichment (CQRS layout, business rules: "AI is additive, not gating"). Unlike
/// <c>SubmitWritingCommandHandler</c> (which performs the AI kickoff directly rather than via a domain
/// event, so its <c>202</c> response can return the operation id from the same transaction), this
/// endpoint's response is only the synchronous deterministic score — there is no operation id to return,
/// so the post-commit event handler is a safe place for the kickoff (see the event's own remarks).
/// Entitlement/quota are checked here, not in the command handler: missing either silently skips
/// enrichment rather than failing the already-committed submission.
/// </summary>
public sealed class SentenceAttemptSubmittedHandler(
    IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs)
    : INotificationHandler<DomainEventNotification<SentenceAttemptSubmittedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<SentenceAttemptSubmittedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // INV-03: entitlement check precedes any AI call. Additive, not gating — a miss here just
        // means the attempt keeps its deterministic score with no AI feedback, never a failure.
        var entitled = await entitlements
            .HasFeatureAsync(domainEvent.TenantId, AiFeatures.SentenceBuilderAiEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return;
        }

        var quotaCheck = await quotas
            .CheckAsync(domainEvent.TenantId, AiFeatures.SentenceBuilderAiQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return;
        }

        var consumeResult = await quotas
            .ConsumeAsync(domainEvent.TenantId, AiFeatures.SentenceBuilderAiQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return;
        }

        var tenantId = domainEvent.TenantId;
        var attemptId = domainEvent.SentenceAttemptId;
        backgroundJobs.Enqueue<SentenceBuilderAiJob>(job => job.ExecuteAsync(tenantId, attemptId, CancellationToken.None));
    }
}
