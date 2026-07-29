using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Billing;

/// <summary>
/// Anti-corruption layer over the payment provider (13 §10) — the domain speaks in Product/Plan/
/// Price/Subscription/Invoice; this port is the only place that knows a provider exists. Implemented
/// by <c>IELTSLms.Infrastructure</c> against Stripe; Application/Domain never reference the provider SDK.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Creates a provider customer if <paramref name="existingCustomerId"/> is null, otherwise returns it unchanged.</summary>
    Task<string> EnsureCustomerAsync(
        string? existingCustomerId, string email, string? name, CancellationToken cancellationToken = default);

    /// <summary>Creates a Checkout Session and returns its hosted URL for client redirect.</summary>
    Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(string providerSubscriptionId, bool atPeriodEnd, CancellationToken cancellationToken = default);

    Task ResumeSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the provider's mirror of a <see cref="Product"/>. Returns the provider product id.</summary>
    Task<string> SyncProductAsync(string internalCode, string name, string? description, CancellationToken cancellationToken = default);

    /// <summary>Creates the provider's mirror of a <see cref="Price"/>. Returns the provider price id. Provider prices are immutable once created.</summary>
    Task<string> SyncPriceAsync(
        string providerProductId, long unitAmountMinorUnits, string currency, BillingInterval interval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the provider's current view of a subscription for reconciliation (13 §12: catches
    /// missed webhooks). Returns <see langword="null"/> if the provider has no such subscription.
    /// </summary>
    Task<ProviderSubscriptionSnapshot?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

    /// <summary>Creates a Billing Portal session and returns its hosted URL — the primary self-service surface (ADR-032).</summary>
    Task<string> CreateBillingPortalSessionAsync(
        string providerCustomerId, string returnUrl, CancellationToken cancellationToken = default);

    /// <summary>Creates a SetupIntent for collecting a new payment method client-side (PCI-compliant: card data never reaches our server). Returns the client secret.</summary>
    Task<string> CreateSetupIntentAsync(string providerCustomerId, CancellationToken cancellationToken = default);

    Task DetachPaymentMethodAsync(string providerPaymentMethodId, CancellationToken cancellationToken = default);

    Task SetDefaultPaymentMethodAsync(
        string providerCustomerId, string providerPaymentMethodId, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing subscription's price immediately, with Stripe computing proration (13 §6).</summary>
    Task ChangeSubscriptionPriceAsync(
        string providerSubscriptionId, string providerPriceId, CancellationToken cancellationToken = default);

    /// <summary>Issues a full or partial refund against a payment intent. Returns the provider refund id.</summary>
    Task<string> CreateRefundAsync(
        string providerPaymentIntentId, long amountMinorUnits, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates the provider's mirror of a <see cref="Coupon"/>. Returns the provider coupon id.</summary>
    Task<string> SyncCouponAsync(Coupon coupon, CancellationToken cancellationToken = default);

    /// <summary>Re-attempts payment on an open/past-due invoice (dunning recovery, 13 §12).</summary>
    Task RetryInvoicePaymentAsync(string providerInvoiceId, CancellationToken cancellationToken = default);
}

/// <summary>A minimal, provider-agnostic snapshot of a subscription's state at the provider.</summary>
public sealed record ProviderSubscriptionSnapshot(
    string Status, DateTimeOffset CurrentPeriodStart, DateTimeOffset CurrentPeriodEnd);

/// <summary>Parameters for creating a Stripe Checkout Session (13 §7/§11/§9).</summary>
public sealed record CreateCheckoutSessionRequest(
    string ProviderCustomerId,
    string ProviderPriceId,
    string SuccessUrl,
    string CancelUrl,
    int TrialPeriodDays,
    IReadOnlyDictionary<string, string> Metadata,
    string? ProviderCouponId = null,
    bool EnableAutomaticTax = true);
