using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Reading;

internal sealed class ReadingAttemptRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ReadingAttempt, ReadingAttemptId>(dbContext), IReadingAttemptRepository
{
    public async Task<IReadOnlyList<ReadingAttempt>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(a => a.TenantId == tenantId && a.UserId == userId && a.SubmittedAtUtc != null)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
