using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Speaking;

internal sealed class SpeakingAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SpeakingAttempt, SpeakingAttemptId>(dbContext), ISpeakingAttemptRepository
{
    public async Task<IReadOnlyList<SpeakingAttempt>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => a.TenantId == tenantId && a.UserId == userId && a.Status != SpeakingAttemptStatus.Recorded)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
