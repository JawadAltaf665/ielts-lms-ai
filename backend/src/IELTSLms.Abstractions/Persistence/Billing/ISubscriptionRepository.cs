using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Subscription"/> (ERD ENT-040). One active subscription per tenant.</summary>
public interface ISubscriptionRepository : IRepository<Subscription, SubscriptionId>
{
    /// <summary>The tenant's current (non-expired) subscription, regardless of status.</summary>
    Task<Subscription?> GetCurrentByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Trialing subscriptions whose trial ends before <paramref name="before"/>, for the trial-ending reminder job.</summary>
    Task<IReadOnlyList<Subscription>> ListTrialsEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default);

    /// <summary>Past-due subscriptions whose grace period elapsed before <paramref name="before"/>, for the suspension sweep.</summary>
    Task<IReadOnlyList<Subscription>> ListPastDueBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default);

    /// <summary>Cancelled/suspended subscriptions whose period ended before <paramref name="before"/>, for the expiration sweep.</summary>
    Task<IReadOnlyList<Subscription>> ListExpirableBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default);

    /// <summary>Non-terminal subscriptions still linked to a Stripe subscription, for the reconciliation job.</summary>
    Task<IReadOnlyList<Subscription>> ListLinkedToProviderAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// All non-expired subscriptions across every tenant, for financial/analytics reporting
    /// (ADR-032). Callers must already hold an <c>ITenantContextAccessor.BypassTenantFilter()</c>
    /// scope — this method does not bypass tenant isolation itself.
    /// </summary>
    Task<IReadOnlyList<Subscription>> ListAllNonExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>Subscriptions that became Cancelled/Expired within the window, for churn reporting (ADR-032).</summary>
    Task<IReadOnlyList<Subscription>> ListChurnedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
