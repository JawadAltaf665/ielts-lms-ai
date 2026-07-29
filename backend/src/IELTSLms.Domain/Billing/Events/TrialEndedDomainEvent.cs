using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a trialing subscription converts to active (paid).</summary>
public sealed record TrialEndedDomainEvent(Guid TenantId, Guid SubscriptionId) : DomainEvent;
