using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.CreateCheckoutSession;

/// <summary>Starts a Stripe Checkout session for a paid plan (13 §7, §11).</summary>
/// <param name="TenantId">The resolved tenant for the current request.</param>
/// <param name="Email">Billing contact email — used to create/reuse the Stripe customer.</param>
/// <param name="Name">Optional billing contact name.</param>
/// <param name="PlanId">The plan being purchased.</param>
/// <param name="PriceId">The specific price (interval/currency) being purchased.</param>
/// <param name="SuccessUrl">Where Stripe redirects on successful checkout.</param>
/// <param name="CancelUrl">Where Stripe redirects if checkout is abandoned.</param>
/// <param name="CouponCode">Optional coupon code to apply (13 §7, ADR-032).</param>
public sealed record CreateCheckoutSessionCommand(
    Guid TenantId,
    string Email,
    string? Name,
    Guid PlanId,
    Guid PriceId,
    string SuccessUrl,
    string CancelUrl,
    string? CouponCode = null) : ICommand<CheckoutSessionResponse>;

public sealed record CheckoutSessionResponse(string Url);
