using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Writing;

internal sealed class WritingSubmissionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WritingSubmission, WritingSubmissionId>(dbContext), IWritingSubmissionRepository
{
    public async Task<IReadOnlyList<WritingSubmission>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(s => s.TenantId == tenantId && s.UserId == userId && s.Status != WritingSubmissionStatus.Draft)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
