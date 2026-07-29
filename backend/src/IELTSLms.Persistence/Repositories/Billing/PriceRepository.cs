using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PriceRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Price, PriceId>(dbContext), IPriceRepository
{
    public async Task<IReadOnlyList<Price>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.PlanId == planId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Price>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.IsActive).ToListAsync(cancellationToken).ConfigureAwait(false);
}
