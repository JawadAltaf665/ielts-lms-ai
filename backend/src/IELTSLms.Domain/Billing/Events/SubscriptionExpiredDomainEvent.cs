using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a subscription's period ends without renewal or reactivation.</summary>
public sealed record SubscriptionExpiredDomainEvent(Guid TenantId, Guid SubscriptionId) : DomainEvent;
