using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetFinancialSummary;

/// <summary>
/// Cross-tenant aggregation: bypasses the tenant query filter for the duration of the query (ADR-032)
/// — this is an administrator-only report; authorization is enforced at the API layer.
/// </summary>
public sealed class GetFinancialSummaryQueryHandler(
    ISubscriptionRepository subscriptions, IInvoiceRepository invoices, IPaymentRepository payments,
    IRefundRepository refunds, ICouponRedemptionRepository couponRedemptions, IPlanRepository plans,
    IPriceRepository prices, ITenantContextAccessor tenantContext)
    : IQueryHandler<GetFinancialSummaryQuery, FinancialSummaryResponse>
{
    public async Task<Result<FinancialSummaryResponse>> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        using var bypass = tenantContext.BypassTenantFilter();

        var paidInvoices = await invoices.ListPaidBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var totalRevenue = paidInvoices.Sum(i => i.TotalMinorUnits);

        var activeSubscriptions = (await subscriptions.ListAllNonExpiredAsync(cancellationToken).ConfigureAwait(false))
            .Where(s => s.Status is SubscriptionStatus.Active or SubscriptionStatus.Trial or SubscriptionStatus.PastDue)
            .ToArray();

        var allPlans = (await plans.ListActiveAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var allPrices = (await prices.ListActiveAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(p => p.Id);

        long mrr = 0;
        var revenueByPlan = new Dictionary<string, long>();
        foreach (var subscription in activeSubscriptions)
        {
            if (!allPrices.TryGetValue(subscription.PriceId, out var price))
            {
                continue;
            }

            var monthlyAmount = price.Interval is BillingInterval.Yearly ? price.Amount.AmountMinorUnits / 12 : price.Amount.AmountMinorUnits;
            mrr += monthlyAmount;

            var planCode = allPlans.TryGetValue(subscription.PlanId, out var plan) ? plan.Code.ToString() : "Unknown";
            revenueByPlan[planCode] = revenueByPlan.GetValueOrDefault(planCode) + monthlyAmount;
        }

        var trialConversions = activeSubscriptions.Count(s => s.TrialEndsAtUtc is { } end && end >= request.From && end < request.To);
        var churned = await subscriptions.ListChurnedBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);

        var periodPayments = await payments.ListBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var failedPayments = periodPayments.Count(p => p.Status is PaymentStatus.Failed);

        var periodRefunds = await refunds.ListBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var refundsTotal = periodRefunds.Where(r => r.Status is RefundStatus.Succeeded).Sum(r => r.AmountMinorUnits);

        var couponRedemptionsCount = await couponRedemptions.CountBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);

        return Result.Success(new FinancialSummaryResponse(
            totalRevenue,
            mrr,
            mrr * 12,
            activeSubscriptions.Length,
            trialConversions,
            churned.Count,
            failedPayments,
            refundsTotal,
            couponRedemptionsCount,
            revenueByPlan));
    }
}
