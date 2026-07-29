using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class QuotaDefinitionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<QuotaDefinition, QuotaDefinitionId>(dbContext), IQuotaDefinitionRepository
{
    public async Task<QuotaDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(q => q.Code == code, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<QuotaDefinition>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await Set.ToListAsync(cancellationToken).ConfigureAwait(false);
}
