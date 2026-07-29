using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetFinancialSummary;

/// <summary>
/// Cross-tenant financial report for administrators (Sprint 2 §12): revenue, MRR/ARR, active
/// subscriptions, trial conversions, churn, failed payments, refunds, coupon usage, revenue by plan.
/// </summary>
public sealed record GetFinancialSummaryQuery(DateTimeOffset From, DateTimeOffset To)
    : IQuery<FinancialSummaryResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["PlatformAdmin"];
}

public sealed record FinancialSummaryResponse(
    long TotalRevenueMinorUnits,
    long MrrMinorUnits,
    long ArrMinorUnits,
    int ActiveSubscriptionsCount,
    int TrialConversionsCount,
    int ChurnedSubscriptionsCount,
    int FailedPaymentsCount,
    long RefundsTotalMinorUnits,
    int CouponRedemptionsCount,
    IReadOnlyDictionary<string, long> RevenueByPlanMinorUnits);
