using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class FeatureGroupRepository(ApplicationDbContext dbContext)
    : RepositoryBase<FeatureGroup, FeatureGroupId>(dbContext), IFeatureGroupRepository
{
    public async Task<IReadOnlyList<FeatureGroup>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await Set.OrderBy(g => g.DisplayOrder).ToListAsync(cancellationToken).ConfigureAwait(false);
}
