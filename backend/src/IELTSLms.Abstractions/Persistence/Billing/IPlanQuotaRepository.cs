using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="PlanQuota"/> — the plan-level quota matrix (ERD ENT-039I).</summary>
public interface IPlanQuotaRepository : IRepository<PlanQuota, PlanQuotaId>
{
    Task<IReadOnlyList<PlanQuota>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default);

    Task<PlanQuota?> GetAsync(PlanId planId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default);
}
