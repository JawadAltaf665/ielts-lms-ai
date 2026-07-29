using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class FeatureRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Feature, FeatureId>(dbContext), IFeatureRepository
{
    public async Task<Feature?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(f => f.Code == code, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Feature>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await Set.ToListAsync(cancellationToken).ConfigureAwait(false);
}
