using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.ValidateCoupon;

/// <summary>Checks a coupon's validity window, redemption cap, and tenant eligibility (13 §7).</summary>
public sealed record ValidateCouponQuery(Guid TenantId, string Code) : IQuery<CouponValidationResponse>;

public sealed record CouponValidationResponse(string Code, string Name, string DiscountType, long Value, string? Currency);
