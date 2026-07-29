using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetSubscription;

/// <summary>Returns the calling tenant's current subscription (13 §4).</summary>
/// <param name="TenantId">The resolved tenant for the current request.</param>
public sealed record GetSubscriptionQuery(Guid TenantId) : IQuery<SubscriptionResponse>;

public sealed record SubscriptionResponse(
    Guid Id,
    Guid PlanId,
    Guid PriceId,
    string Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    DateTimeOffset? TrialEndsAtUtc,
    bool CancelAtPeriodEnd);
