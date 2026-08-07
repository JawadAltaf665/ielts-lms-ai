using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Learning;

internal sealed class DailyPlanRepository(ApplicationDbContext dbContext)
    : RepositoryBase<DailyPlan, DailyPlanId>(dbContext), IDailyPlanRepository
{
    public async Task<DailyPlan?> GetByUserAndDateAsync(
        Guid tenantId, Guid userId, DateOnly planDate, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.UserId == userId && p.PlanDate == planDate, cancellationToken)
            .ConfigureAwait(false);
}
