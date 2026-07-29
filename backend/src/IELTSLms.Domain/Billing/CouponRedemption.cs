using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Records a <see cref="Coupon"/> applied to a <see cref="Subscription"/> — the "Discount" instance
/// (ERD ENT-039R, ADR-032, 13 §7). At most one active redemption per subscription.
/// </summary>
public sealed class CouponRedemption : TenantAuditableEntity<CouponRedemptionId>
{
    private CouponRedemption(CouponRedemptionId id, Guid tenantId, CouponId couponId, SubscriptionId subscriptionId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        CouponId = couponId;
        SubscriptionId = subscriptionId;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private CouponRedemption()
    {
    }

    public CouponId CouponId { get; private set; }

    public SubscriptionId SubscriptionId { get; private set; }

    public bool IsActive { get; private set; }

    public static CouponRedemption Create(
        CouponRedemptionId id, Guid tenantId, CouponId couponId, SubscriptionId subscriptionId, DateTimeOffset nowUtc) =>
        new(id, tenantId, couponId, subscriptionId, nowUtc);

    public Result Revoke(DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
