using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PlanQuotaRepository(ApplicationDbContext dbContext)
    : RepositoryBase<PlanQuota, PlanQuotaId>(dbContext), IPlanQuotaRepository
{
    public async Task<IReadOnlyList<PlanQuota>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default) =>
        await Set.Where(pq => pq.PlanId == planId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<PlanQuota?> GetAsync(PlanId planId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(pq => pq.PlanId == planId && pq.QuotaDefinitionId == quotaDefinitionId, cancellationToken)
            .ConfigureAwait(false);
}
