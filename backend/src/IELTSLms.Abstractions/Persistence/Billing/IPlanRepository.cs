using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Plan"/> (ERD ENT-039B). Global catalog, not tenant-scoped.</summary>
public interface IPlanRepository : IRepository<Plan, PlanId>
{
    Task<Plan?> GetByCodeAsync(PlanCode code, CancellationToken cancellationToken = default);

    /// <summary>Active plans ordered by <see cref="Plan.DisplayOrder"/> (lowest tier first).</summary>
    Task<IReadOnlyList<Plan>> ListActiveAsync(CancellationToken cancellationToken = default);
}
