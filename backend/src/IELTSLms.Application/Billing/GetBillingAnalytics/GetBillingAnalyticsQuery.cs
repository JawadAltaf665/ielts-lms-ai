using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Billing.GetBillingAnalytics;

/// <summary>
/// Cross-tenant operational analytics for administrators (Sprint 2 §13): active customers/
/// subscriptions, payment success rate, churn rate, an ARPA-based LTV estimate, and current-period
/// quota utilization by quota code.
/// </summary>
public sealed record GetBillingAnalyticsQuery(DateTimeOffset From, DateTimeOffset To)
    : IQuery<BillingAnalyticsResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["PlatformAdmin"];
}

public sealed record BillingAnalyticsResponse(
    int ActiveCustomersCount,
    int ActiveSubscriptionsCount,
    double PaymentSuccessRate,
    double ChurnRate,
    long EstimatedLifetimeValueMinorUnits,
    IReadOnlyDictionary<string, long> QuotaUtilizationByCode);
