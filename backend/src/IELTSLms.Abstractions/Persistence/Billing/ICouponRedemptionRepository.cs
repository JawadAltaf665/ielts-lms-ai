using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="CouponRedemption"/> (ERD ENT-039R).</summary>
public interface ICouponRedemptionRepository : IRepository<CouponRedemption, CouponRedemptionId>
{
    Task<CouponRedemption?> GetActiveBySubscriptionAsync(SubscriptionId subscriptionId, CancellationToken cancellationToken = default);

    Task<int> CountByCouponAsync(CouponId couponId, CancellationToken cancellationToken = default);

    /// <summary>Redemptions recorded within the window, across every tenant, for reporting (ADR-032). Requires an active tenant-filter bypass.</summary>
    Task<int> CountBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
