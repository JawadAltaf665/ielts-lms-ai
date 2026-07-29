using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetBillingAnalytics;

/// <summary>Cross-tenant aggregation: bypasses the tenant query filter for the duration of the query (ADR-032).</summary>
public sealed class GetBillingAnalyticsQueryHandler(
    ISubscriptionRepository subscriptions, IPaymentRepository payments, IInvoiceRepository invoices,
    IUsageRecordRepository usageRecords, IQuotaDefinitionRepository quotaDefinitions,
    ITenantContextAccessor tenantContext)
    : IQueryHandler<GetBillingAnalyticsQuery, BillingAnalyticsResponse>
{
    public async Task<Result<BillingAnalyticsResponse>> Handle(GetBillingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        using var bypass = tenantContext.BypassTenantFilter();

        var activeSubscriptions = (await subscriptions.ListAllNonExpiredAsync(cancellationToken).ConfigureAwait(false))
            .Where(s => s.Status is SubscriptionStatus.Active or SubscriptionStatus.Trial or SubscriptionStatus.PastDue)
            .ToArray();

        var churned = await subscriptions.ListChurnedBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var churnRate = activeSubscriptions.Length + churned.Count == 0
            ? 0
            : (double)churned.Count / (activeSubscriptions.Length + churned.Count);

        var periodPayments = await payments.ListBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var succeeded = periodPayments.Count(p => p.Status is PaymentStatus.Succeeded);
        var attempted = periodPayments.Count(p => p.Status is PaymentStatus.Succeeded or PaymentStatus.Failed);
        var successRate = attempted == 0 ? 0 : (double)succeeded / attempted;

        // ARPA-based estimate, not a cohort-tracked LTV — a simplified stand-in until real cohort
        // retention data exists (ADR-032 notes financial reporting as computed-on-demand, no new
        // materialized/cohort tables this sprint).
        var paidInvoices = await invoices.ListPaidBetweenAsync(request.From, request.To, cancellationToken).ConfigureAwait(false);
        var arpa = activeSubscriptions.Length == 0 ? 0 : paidInvoices.Sum(i => i.TotalMinorUnits) / activeSubscriptions.Length;
        var estimatedLtv = arpa * 12;

        var periodStart = new DateTimeOffset(request.From.Year, request.From.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var usage = await usageRecords.ListByPeriodAsync(periodStart, cancellationToken).ConfigureAwait(false);
        var definitions = (await quotaDefinitions.ListAllAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(d => d.Id, d => d.Code);

        var utilizationByCode = usage
            .GroupBy(u => definitions.GetValueOrDefault(u.QuotaDefinitionId, "unknown"))
            .ToDictionary(g => g.Key, g => g.Sum(u => u.Consumed));

        return Result.Success(new BillingAnalyticsResponse(
            activeSubscriptions.Length,
            activeSubscriptions.Length,
            successRate,
            churnRate,
            estimatedLtv,
            utilizationByCode));
    }
}
