using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PlanFeatureRepository(ApplicationDbContext dbContext)
    : RepositoryBase<PlanFeature, PlanFeatureId>(dbContext), IPlanFeatureRepository
{
    public async Task<IReadOnlyList<PlanFeature>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default) =>
        await Set.Where(pf => pf.PlanId == planId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<PlanFeature?> GetAsync(PlanId planId, FeatureId featureId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(pf => pf.PlanId == planId && pf.FeatureId == featureId, cancellationToken).ConfigureAwait(false);
}
