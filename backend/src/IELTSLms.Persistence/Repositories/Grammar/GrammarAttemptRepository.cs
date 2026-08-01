using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarAttempt, GrammarAttemptId>(dbContext), IGrammarAttemptRepository
{
    public async Task<IReadOnlyList<GrammarAttempt>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => a.TenantId == tenantId && a.UserId == userId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
