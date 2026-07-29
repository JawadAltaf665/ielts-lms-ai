using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Billing.Events;

/// <summary>Raised when a Stripe Checkout session completes and activates its subscription.</summary>
public sealed record CheckoutCompletedDomainEvent(Guid TenantId, Guid SubscriptionId) : DomainEvent;
