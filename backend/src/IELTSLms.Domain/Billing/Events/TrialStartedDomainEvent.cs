using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a subscription enters its trial period.</summary>
public sealed record TrialStartedDomainEvent(Guid TenantId, Guid SubscriptionId, DateTimeOffset TrialEndsAtUtc) : DomainEvent;
