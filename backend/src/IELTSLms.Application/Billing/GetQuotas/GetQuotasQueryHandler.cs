using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetQuotas;

/// <summary>
/// Resolves the full quota catalog for the tenant: TenantQuota override ?? PlanQuota on the tenant's
/// current plan ?? 0 (13 §3), joined against the current period's <see cref="UsageRecord"/>.
/// </summary>
public sealed class GetQuotasQueryHandler(
    IQuotaDefinitionRepository quotaDefinitions,
    IPlanQuotaRepository planQuotas,
    ITenantQuotaRepository tenantQuotas,
    IUsageRecordRepository usageRecords,
    ISubscriptionRepository subscriptions,
    IClock clock)
    : IQueryHandler<GetQuotasQuery, IReadOnlyList<QuotaStatusResponse>>
{
    public async Task<Result<IReadOnlyList<QuotaStatusResponse>>> Handle(GetQuotasQuery request, CancellationToken cancellationToken)
    {
        var definitions = await quotaDefinitions.ListAllAsync(cancellationToken).ConfigureAwait(false);

        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var planLimits = subscription is null
            ? new Dictionary<QuotaDefinitionId, long>()
            : (await planQuotas.ListByPlanAsync(subscription.PlanId, cancellationToken).ConfigureAwait(false))
                .ToDictionary(pq => pq.QuotaDefinitionId, pq => pq.Limit);

        var overrides = (await tenantQuotas.ListByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false))
            .ToDictionary(tq => tq.QuotaDefinitionId, tq => tq.Limit);

        var periodStart = CurrentPeriodStart(clock.UtcNow);
        var usage = (await usageRecords.ListByTenantAsync(request.TenantId, periodStart, cancellationToken).ConfigureAwait(false))
            .ToDictionary(u => u.QuotaDefinitionId, u => u.Consumed);

        IReadOnlyList<QuotaStatusResponse> responses = definitions
            .Select(d =>
            {
                var limit = overrides.TryGetValue(d.Id, out var overrideLimit)
                    ? overrideLimit
                    : planLimits.GetValueOrDefault(d.Id, 0);
                var consumed = usage.GetValueOrDefault(d.Id, 0);
                var remaining = limit == PlanQuota.Unlimited ? long.MaxValue : Math.Max(0, limit - consumed);
                return new QuotaStatusResponse(d.Code, d.Name, d.Unit, limit, consumed, remaining);
            })
            .ToArray();

        return Result.Success(responses);
    }

    /// <summary>Start of the current calendar month (UTC) — the period boundary for monthly-reset quotas.</summary>
    private static DateTimeOffset CurrentPeriodStart(DateTimeOffset nowUtc) =>
        new(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
}
