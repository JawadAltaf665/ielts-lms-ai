using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PlanRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Plan, PlanId>(dbContext), IPlanRepository
{
    public async Task<Plan?> GetByCodeAsync(PlanCode code, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(p => p.Code == code, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Plan>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync(cancellationToken).ConfigureAwait(false);
}
