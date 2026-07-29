using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a new subscription is created in the pending state.</summary>
public sealed record SubscriptionCreatedDomainEvent(Guid TenantId, Guid SubscriptionId, Guid PlanId) : DomainEvent;
