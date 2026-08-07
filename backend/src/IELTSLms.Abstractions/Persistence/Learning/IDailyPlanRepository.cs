using IELTSLms.Domain.Learning;

namespace IELTSLms.Abstractions.Persistence.Learning;

/// <summary>Aggregate access for <see cref="DailyPlan"/> (ERD ENT-034).</summary>
public interface IDailyPlanRepository : IRepository<DailyPlan, DailyPlanId>
{
    Task<DailyPlan?> GetByUserAndDateAsync(
        Guid tenantId, Guid userId, DateOnly planDate, CancellationToken cancellationToken = default);
}
