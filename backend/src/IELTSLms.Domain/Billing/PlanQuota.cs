using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Billing;

/// <summary>Default limit for a <see cref="QuotaDefinition"/> on a <see cref="Plan"/> (ERD ENT-039I).</summary>
public sealed class PlanQuota : AuditableEntity<PlanQuotaId>
{
    /// <summary>Sentinel <see cref="Limit"/> value meaning no cap.</summary>
    public const long Unlimited = -1;

    private PlanQuota(PlanQuotaId id, PlanId planId, QuotaDefinitionId quotaDefinitionId, long limit, DateTimeOffset nowUtc)
        : base(id)
    {
        PlanId = planId;
        QuotaDefinitionId = quotaDefinitionId;
        Limit = limit;
        CreatedAtUtc = nowUtc;
    }

    private PlanQuota()
    {
    }

    public PlanId PlanId { get; private set; }

    public QuotaDefinitionId QuotaDefinitionId { get; private set; }

    public long Limit { get; private set; }

    public static Result<PlanQuota> Create(
        PlanQuotaId id, PlanId planId, QuotaDefinitionId quotaDefinitionId, long limit, DateTimeOffset nowUtc)
    {
        if (limit < Unlimited)
        {
            return Error.Validation("VALIDATION_QUOTA_LIMIT", "Quota limit must be -1 (unlimited) or greater.");
        }

        return new PlanQuota(id, planId, quotaDefinitionId, limit, nowUtc);
    }

    public Result UpdateLimit(long limit, DateTimeOffset nowUtc)
    {
        if (limit < Unlimited)
        {
            return Result.Failure(Error.Validation("VALIDATION_QUOTA_LIMIT", "Quota limit must be -1 (unlimited) or greater."));
        }

        Limit = limit;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
