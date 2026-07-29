using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class TenantFeatureRepository(ApplicationDbContext dbContext)
    : RepositoryBase<TenantFeature, TenantFeatureId>(dbContext), ITenantFeatureRepository
{
    public async Task<TenantFeature?> GetAsync(Guid tenantId, FeatureId featureId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FeatureId == featureId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<TenantFeature>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.Where(tf => tf.TenantId == tenantId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
