using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarProgressRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarProgress, GrammarProgressId>(dbContext), IGrammarProgressRepository
{
    public async Task<GrammarProgress?> GetByUserAndTopicAsync(
        Guid tenantId, Guid userId, GrammarTopicId topicId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.UserId == userId && p.GrammarTopicId == topicId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<GrammarProgress>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
