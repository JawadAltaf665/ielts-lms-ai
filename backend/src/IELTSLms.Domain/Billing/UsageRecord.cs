using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Metered consumption counter for a tenant/quota/period (ERD ENT-039K). One row per
/// (TenantId, QuotaDefinitionId, PeriodStart); reset by the quota-reset background job.
/// </summary>
public sealed class UsageRecord : TenantAuditableEntity<UsageRecordId>
{
    private UsageRecord(
        UsageRecordId id, Guid tenantId, QuotaDefinitionId quotaDefinitionId,
        DateTimeOffset periodStart, DateTimeOffset periodEnd, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        QuotaDefinitionId = quotaDefinitionId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Consumed = 0;
        CreatedAtUtc = nowUtc;
    }

    private UsageRecord()
    {
    }

    public QuotaDefinitionId QuotaDefinitionId { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }

    public DateTimeOffset PeriodEnd { get; private set; }

    public long Consumed { get; private set; }

    public static UsageRecord Create(
        UsageRecordId id, Guid tenantId, QuotaDefinitionId quotaDefinitionId,
        DateTimeOffset periodStart, DateTimeOffset periodEnd, DateTimeOffset nowUtc) =>
        new(id, tenantId, quotaDefinitionId, periodStart, periodEnd, nowUtc);

    public Result Consume(long amount, DateTimeOffset nowUtc)
    {
        if (amount <= 0)
        {
            return Result.Failure(Error.Validation("VALIDATION_USAGE_AMOUNT", "Usage amount must be positive."));
        }

        Consumed += amount;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
