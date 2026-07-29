using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a subscription transitions into the active state.</summary>
public sealed record SubscriptionActivatedDomainEvent(Guid TenantId, Guid SubscriptionId) : DomainEvent;
