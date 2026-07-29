using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.ValidateCoupon;

public sealed class ValidateCouponQueryHandler(ICouponRepository coupons, IClock clock)
    : IQueryHandler<ValidateCouponQuery, CouponValidationResponse>
{
    public async Task<Result<CouponValidationResponse>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await coupons.GetByCodeAsync(request.Code.Trim().ToUpperInvariant(), cancellationToken).ConfigureAwait(false);
        if (coupon is null)
        {
            return Result.Failure<CouponValidationResponse>(BillingErrors.CouponNotFound);
        }

        var eligibility = coupon.CheckEligibility(request.TenantId, clock.UtcNow);
        if (eligibility.IsFailure)
        {
            return Result.Failure<CouponValidationResponse>(eligibility.Error);
        }

        return Result.Success(new CouponValidationResponse(
            coupon.Code, coupon.Name, coupon.DiscountType.ToString(), coupon.Value, coupon.Currency));
    }
}
