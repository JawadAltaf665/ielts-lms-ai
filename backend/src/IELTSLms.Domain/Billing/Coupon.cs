using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// A promotional discount definition, mirrors a Stripe Coupon (ERD ENT-043, ADR-032). Global catalog
/// by default; <see cref="TenantId"/> restricts it to one tenant when set.
/// </summary>
public sealed class Coupon : AuditableEntity<CouponId>
{
    /// <summary>Basis points per whole percent — <see cref="Value"/> stores percentages as an integer, matching the codebase's minor-units-only money convention (no floating point).</summary>
    public const int BasisPointsPerPercent = 100;

    private Coupon(
        CouponId id, string code, string name, CouponDiscountType discountType, long value, string? currency,
        DateTimeOffset validFrom, DateTimeOffset? validTo, int? maxRedemptions, Guid? tenantId, DateTimeOffset nowUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        DiscountType = discountType;
        Value = value;
        Currency = currency;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxRedemptions = maxRedemptions;
        TenantId = tenantId;
        RedemptionCount = 0;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    private Coupon()
    {
        Code = null!;
        Name = null!;
    }

    public const int CodeMaxLength = 64;
    public const int NameMaxLength = 256;

    public string Code { get; private set; }

    public string Name { get; private set; }

    public CouponDiscountType DiscountType { get; private set; }

    /// <summary>
    /// For <see cref="CouponDiscountType.Percentage"/>: basis points (100 = 1%, 10000 = 100%).
    /// For <see cref="CouponDiscountType.Fixed"/>: minor currency units in <see cref="Currency"/>.
    /// </summary>
    public long Value { get; private set; }

    /// <summary>Required for <see cref="CouponDiscountType.Fixed"/>; unset for <see cref="CouponDiscountType.Percentage"/>.</summary>
    public string? Currency { get; private set; }

    public DateTimeOffset ValidFrom { get; private set; }

    public DateTimeOffset? ValidTo { get; private set; }

    public int? MaxRedemptions { get; private set; }

    public int RedemptionCount { get; private set; }

    /// <summary>Restricts the coupon to one tenant when set; a global/campaign coupon otherwise (13 §7).</summary>
    public Guid? TenantId { get; private set; }

    public bool IsActive { get; private set; }

    public string? StripeCouponId { get; private set; }

    public static Result<Coupon> Create(
        CouponId id, string code, string name, CouponDiscountType discountType, long value, string? currency,
        DateTimeOffset validFrom, DateTimeOffset? validTo, int? maxRedemptions, Guid? tenantId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Coupon code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Coupon name is required.");
        }

        if (value <= 0)
        {
            return Error.Validation("VALIDATION_COUPON_VALUE", "Coupon value must be positive.");
        }

        if (discountType is CouponDiscountType.Percentage)
        {
            if (value > 100 * BasisPointsPerPercent)
            {
                return Error.Validation("VALIDATION_COUPON_PERCENTAGE", "Percentage discount cannot exceed 100%.");
            }
        }
        else if (string.IsNullOrWhiteSpace(currency))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Currency is required for a fixed-amount coupon.");
        }

        if (validTo is not null && validTo <= validFrom)
        {
            return Error.Validation("VALIDATION_COUPON_VALIDITY", "Coupon validTo must be after validFrom.");
        }

        if (maxRedemptions is <= 0)
        {
            return Error.Validation("VALIDATION_COUPON_MAX_REDEMPTIONS", "Max redemptions must be positive when set.");
        }

        return new Coupon(
            id, code.Trim().ToUpperInvariant(), name.Trim(), discountType, value,
            discountType is CouponDiscountType.Fixed ? currency!.Trim().ToUpperInvariant() : null,
            validFrom, validTo, maxRedemptions, tenantId, nowUtc);
    }

    /// <summary>Validity window + redemption cap + tenant eligibility, independent of any specific subscription.</summary>
    public Result CheckEligibility(Guid tenantId, DateTimeOffset nowUtc)
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict("BILLING_COUPON_INACTIVE", "This coupon is no longer active."));
        }

        if (nowUtc < ValidFrom || (ValidTo is not null && nowUtc > ValidTo))
        {
            return Result.Failure(Error.Conflict("BILLING_COUPON_EXPIRED", "This coupon is not currently valid."));
        }

        if (MaxRedemptions is { } max && RedemptionCount >= max)
        {
            return Result.Failure(Error.Conflict("BILLING_COUPON_REDEMPTION_CAP", "This coupon has reached its redemption limit."));
        }

        if (TenantId is { } restrictedTenantId && restrictedTenantId != tenantId)
        {
            return Result.Failure(Error.Forbidden("BILLING_COUPON_NOT_ELIGIBLE", "This coupon is not available for this tenant."));
        }

        return Result.Success();
    }

    public void RecordRedemption(DateTimeOffset nowUtc)
    {
        RedemptionCount++;
        UpdatedAtUtc = nowUtc;
    }

    public void AttachStripeCoupon(string stripeCouponId, DateTimeOffset nowUtc)
    {
        StripeCouponId = stripeCouponId;
        UpdatedAtUtc = nowUtc;
    }

    public Result Deactivate(DateTimeOffset nowUtc)
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
