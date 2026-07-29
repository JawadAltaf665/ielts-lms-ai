using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when an active subscription moves to a different plan (upgrade/downgrade).</summary>
public sealed record PlanChangedDomainEvent(Guid TenantId, Guid SubscriptionId, Guid OldPlanId, Guid NewPlanId) : DomainEvent;
