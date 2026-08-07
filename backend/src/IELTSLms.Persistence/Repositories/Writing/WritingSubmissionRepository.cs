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

    public async Task<IReadOnlyList<WritingSubmission>> ListSubmittedByTaskAsync(
        Guid tenantId, WritingTaskId taskId, WritingSubmissionId excludingSubmissionId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(s =>
                s.TenantId == tenantId && s.WritingTaskId == taskId && s.Id != excludingSubmissionId
                && s.Status != WritingSubmissionStatus.Draft)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
