using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class UsageRecordRepository(ApplicationDbContext dbContext)
    : RepositoryBase<UsageRecord, UsageRecordId>(dbContext), IUsageRecordRepository
{
    public async Task<UsageRecord?> GetAsync(
        Guid tenantId, QuotaDefinitionId quotaDefinitionId, DateTimeOffset periodStart, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            u => u.TenantId == tenantId && u.QuotaDefinitionId == quotaDefinitionId && u.PeriodStart == periodStart,
            cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<UsageRecord>> ListByTenantAsync(
        Guid tenantId, DateTimeOffset periodStart, CancellationToken cancellationToken = default) =>
        await Set.Where(u => u.TenantId == tenantId && u.PeriodStart == periodStart)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<UsageRecord>> ListEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        await Set.Where(u => u.PeriodEnd < before).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<UsageRecord>> ListByPeriodAsync(DateTimeOffset periodStart, CancellationToken cancellationToken = default) =>
        await Set.Where(u => u.PeriodStart == periodStart).ToListAsync(cancellationToken).ConfigureAwait(false);
}
