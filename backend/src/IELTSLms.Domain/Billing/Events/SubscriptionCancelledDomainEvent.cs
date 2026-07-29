using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when cancellation is requested; access continues until the current period ends.</summary>
public sealed record SubscriptionCancelledDomainEvent(Guid TenantId, Guid SubscriptionId) : DomainEvent;
