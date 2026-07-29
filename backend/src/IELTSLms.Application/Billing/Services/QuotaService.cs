using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.Services;

/// <summary>
/// Application-layer implementation of the <see cref="IQuotaService"/> port other bounded contexts
/// inject to check and record metered usage (13 §3, ADR-031). Resolution: TenantQuota override ??
/// PlanQuota on the tenant's current plan ?? 0.
/// </summary>
public sealed class QuotaService(
    IQuotaDefinitionRepository quotaDefinitions,
    IPlanQuotaRepository planQuotas,
    ITenantQuotaRepository tenantQuotas,
    IUsageRecordRepository usageRecords,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids)
    : IQuotaService
{
    public async Task<QuotaCheckResult> CheckAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default)
    {
        var (limit, consumed) = await ResolveAsync(tenantId, quotaCode, cancellationToken).ConfigureAwait(false);

        if (limit == PlanQuota.Unlimited)
        {
            return new QuotaCheckResult(true, limit, consumed, long.MaxValue);
        }

        var remaining = Math.Max(0, limit - consumed);
        return new QuotaCheckResult(amount <= remaining, limit, consumed, remaining);
    }

    public async Task<Result> ConsumeAsync(Guid tenantId, string quotaCode, long amount, CancellationToken cancellationToken = default)
    {
        var definition = await quotaDefinitions.GetByCodeAsync(quotaCode, cancellationToken).ConfigureAwait(false);
        if (definition is null)
        {
            return Result.Failure(Error.NotFound("BILLING_QUOTA_NOT_FOUND", "The requested quota is not defined."));
        }

        var check = await CheckAsync(tenantId, quotaCode, amount, cancellationToken).ConfigureAwait(false);
        if (!check.Allowed)
        {
            return Result.Failure(BillingErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var periodStart = CurrentPeriodStart(now);
        var record = await usageRecords.GetAsync(tenantId, definition.Id, periodStart, cancellationToken).ConfigureAwait(false);

        if (record is null)
        {
            record = UsageRecord.Create(
                UsageRecordId.From(guids.NewGuid()), tenantId, definition.Id, periodStart, NextPeriodStart(periodStart), now);
            usageRecords.Add(record);
        }

        var consumeResult = record.Consume(amount, now);
        if (consumeResult.IsFailure)
        {
            return consumeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private async Task<(long Limit, long Consumed)> ResolveAsync(Guid tenantId, string quotaCode, CancellationToken cancellationToken)
    {
        var definition = await quotaDefinitions.GetByCodeAsync(quotaCode, cancellationToken).ConfigureAwait(false);
        if (definition is null)
        {
            return (0, 0);
        }

        var overrideQuota = await tenantQuotas.GetAsync(tenantId, definition.Id, cancellationToken).ConfigureAwait(false);
        long limit;
        if (overrideQuota is not null)
        {
            limit = overrideQuota.Limit;
        }
        else
        {
            var subscription = await subscriptions.GetCurrentByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false);
            var planQuota = subscription is null
                ? null
                : await planQuotas.GetAsync(subscription.PlanId, definition.Id, cancellationToken).ConfigureAwait(false);
            limit = planQuota?.Limit ?? 0;
        }

        var periodStart = CurrentPeriodStart(clock.UtcNow);
        var record = await usageRecords.GetAsync(tenantId, definition.Id, periodStart, cancellationToken).ConfigureAwait(false);
        return (limit, record?.Consumed ?? 0);
    }

    private static DateTimeOffset CurrentPeriodStart(DateTimeOffset nowUtc) =>
        new(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset NextPeriodStart(DateTimeOffset periodStart) => periodStart.AddMonths(1);
}
