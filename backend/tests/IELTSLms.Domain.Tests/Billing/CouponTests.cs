using IELTSLms.Domain.Billing;

namespace IELTSLms.Domain.Tests.Billing;

public sealed class CouponTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

    private static Coupon CreatePercentageCoupon(DateTimeOffset validFrom, DateTimeOffset? validTo, int? maxRedemptions = null, Guid? tenantId = null) =>
        Coupon.Create(
            CouponId.From(Guid.NewGuid()), "SAVE10", "10% off", CouponDiscountType.Percentage, 1000, currency: null,
            validFrom, validTo, maxRedemptions, tenantId, Now).Value;

    [Fact]
    public void CheckEligibility_within_window_and_under_cap_succeeds()
    {
        var coupon = CreatePercentageCoupon(Now.AddDays(-1), Now.AddDays(1), maxRedemptions: 5);

        var result = coupon.CheckEligibility(Guid.NewGuid(), Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CheckEligibility_before_valid_from_fails()
    {
        var coupon = CreatePercentageCoupon(Now.AddDays(1), null);

        var result = coupon.CheckEligibility(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_COUPON_EXPIRED", result.Error.Code);
    }

    [Fact]
    public void CheckEligibility_after_valid_to_fails()
    {
        var coupon = CreatePercentageCoupon(Now.AddDays(-10), Now.AddDays(-1));

        var result = coupon.CheckEligibility(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_COUPON_EXPIRED", result.Error.Code);
    }

    [Fact]
    public void CheckEligibility_at_redemption_cap_fails()
    {
        var coupon = CreatePercentageCoupon(Now.AddDays(-1), Now.AddDays(1), maxRedemptions: 1);
        coupon.RecordRedemption(Now);

        var result = coupon.CheckEligibility(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_COUPON_REDEMPTION_CAP", result.Error.Code);
    }

    [Fact]
    public void CheckEligibility_restricted_to_other_tenant_fails()
    {
        var restrictedTenantId = Guid.NewGuid();
        var coupon = CreatePercentageCoupon(Now.AddDays(-1), Now.AddDays(1), tenantId: restrictedTenantId);

        var result = coupon.CheckEligibility(Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BILLING_COUPON_NOT_ELIGIBLE", result.Error.Code);
    }

    [Fact]
    public void CheckEligibility_restricted_to_matching_tenant_succeeds()
    {
        var tenantId = Guid.NewGuid();
        var coupon = CreatePercentageCoupon(Now.AddDays(-1), Now.AddDays(1), tenantId: tenantId);

        var result = coupon.CheckEligibility(tenantId, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_percentage_over_100_fails()
    {
        var result = Coupon.Create(
            CouponId.From(Guid.NewGuid()), "TOOMUCH", "Too much", CouponDiscountType.Percentage, 10001, null,
            Now, null, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_COUPON_PERCENTAGE", result.Error.Code);
    }

    [Fact]
    public void Create_fixed_without_currency_fails()
    {
        var result = Coupon.Create(
            CouponId.From(Guid.NewGuid()), "FIXED10", "10 off", CouponDiscountType.Fixed, 1000, null,
            Now, null, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_REQUIRED", result.Error.Code);
    }
}
