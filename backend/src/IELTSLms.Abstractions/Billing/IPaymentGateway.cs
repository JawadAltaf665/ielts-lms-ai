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
}

/// <summary>A minimal, provider-agnostic snapshot of a subscription's state at the provider.</summary>
public sealed record ProviderSubscriptionSnapshot(
    string Status, DateTimeOffset CurrentPeriodStart, DateTimeOffset CurrentPeriodEnd);

/// <summary>Parameters for creating a Stripe Checkout Session (13 §7/§11).</summary>
public sealed record CreateCheckoutSessionRequest(
    string ProviderCustomerId,
    string ProviderPriceId,
    string SuccessUrl,
    string CancelUrl,
    int TrialPeriodDays,
    IReadOnlyDictionary<string, string> Metadata);
