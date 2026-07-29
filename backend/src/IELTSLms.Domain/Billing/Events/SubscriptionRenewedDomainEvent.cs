using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a successful renewal extends the current billing period.</summary>
public sealed record SubscriptionRenewedDomainEvent(Guid TenantId, Guid SubscriptionId, DateTimeOffset CurrentPeriodEnd) : DomainEvent;
