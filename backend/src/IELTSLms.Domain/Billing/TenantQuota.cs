using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>
/// Tenant-specific override of a quota limit beyond the plan default (ERD ENT-039J).
/// Resolution order: TenantQuota override ?? PlanQuota ?? 0.
/// </summary>
public sealed class TenantQuota : TenantAuditableEntity<TenantQuotaId>
{
    private TenantQuota(TenantQuotaId id, Guid tenantId, QuotaDefinitionId quotaDefinitionId, long limit, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        QuotaDefinitionId = quotaDefinitionId;
        Limit = limit;
        CreatedAtUtc = nowUtc;
    }

    private TenantQuota()
    {
    }

    public QuotaDefinitionId QuotaDefinitionId { get; private set; }

    public long Limit { get; private set; }

    public static Result<TenantQuota> Create(
        TenantQuotaId id, Guid tenantId, QuotaDefinitionId quotaDefinitionId, long limit, DateTimeOffset nowUtc)
    {
        if (limit < PlanQuota.Unlimited)
        {
            return Error.Validation("VALIDATION_QUOTA_LIMIT", "Quota limit must be -1 (unlimited) or greater.");
        }

        return new TenantQuota(id, tenantId, quotaDefinitionId, limit, nowUtc);
    }

    public Result UpdateLimit(long limit, DateTimeOffset nowUtc)
    {
        if (limit < PlanQuota.Unlimited)
        {
            return Result.Failure(Error.Validation("VALIDATION_QUOTA_LIMIT", "Quota limit must be -1 (unlimited) or greater."));
        }

        Limit = limit;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
