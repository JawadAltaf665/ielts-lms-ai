using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Writing;

internal sealed class WritingTaskRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WritingTask, WritingTaskId>(dbContext), IWritingTaskRepository
{
    public async Task<WritingTask?> GetVisibleByIdAsync(Guid tenantId, WritingTaskId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<WritingTask>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, int skip, int take, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, taskType)
            .OrderBy(t => t.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, taskType).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, WritingTaskId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            t => t.TenantId == tenantId && t.Title == title && (!excludingId.HasValue || t.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    private IQueryable<WritingTask> Visible(Guid tenantId, ReadingTrack? track, WritingTaskType? taskType)
    {
        var query = Set.Where(t => t.TenantId == null || t.TenantId == tenantId);

        if (track is { } trackValue)
        {
            query = query.Where(t => t.Track == trackValue);
        }

        if (taskType is { } taskTypeValue)
        {
            query = query.Where(t => t.TaskType == taskTypeValue);
        }

        return query;
    }
}
