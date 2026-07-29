using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Price"/> (ERD ENT-039C). Global catalog, not tenant-scoped.</summary>
public interface IPriceRepository : IRepository<Price, PriceId>
{
    Task<IReadOnlyList<Price>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Price>> ListActiveAsync(CancellationToken cancellationToken = default);
}
