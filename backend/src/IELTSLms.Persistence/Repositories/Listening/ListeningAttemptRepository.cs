using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Listening;

internal sealed class ListeningAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ListeningAttempt, ListeningAttemptId>(dbContext), IListeningAttemptRepository
{
    public async Task<IReadOnlyList<ListeningAttempt>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => a.TenantId == tenantId && a.UserId == userId && a.SubmittedAtUtc != null)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
