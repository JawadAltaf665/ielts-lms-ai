using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class TenantQuotaRepository(ApplicationDbContext dbContext)
    : RepositoryBase<TenantQuota, TenantQuotaId>(dbContext), ITenantQuotaRepository
{
    public async Task<TenantQuota?> GetAsync(Guid tenantId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(tq => tq.TenantId == tenantId && tq.QuotaDefinitionId == quotaDefinitionId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<TenantQuota>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.Where(tq => tq.TenantId == tenantId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
