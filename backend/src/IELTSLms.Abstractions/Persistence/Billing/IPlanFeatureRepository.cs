using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="PlanFeature"/> — the plan-level entitlement matrix (ERD ENT-039F).</summary>
public interface IPlanFeatureRepository : IRepository<PlanFeature, PlanFeatureId>
{
    Task<IReadOnlyList<PlanFeature>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default);

    Task<PlanFeature?> GetAsync(PlanId planId, FeatureId featureId, CancellationToken cancellationToken = default);
}
