using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing.Fakes;

internal sealed class FakeWritingTaskRepository : IWritingTaskRepository
{
    private readonly List<WritingTask> _tasks = [];

    public void Seed(WritingTask task) => _tasks.Add(task);

    public IReadOnlyList<WritingTask> Tasks => _tasks;

    public Task<WritingTask?> GetByIdAsync(WritingTaskId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

    public void Add(WritingTask aggregate) => _tasks.Add(aggregate);

    public void Remove(WritingTask aggregate) => _tasks.Remove(aggregate);

    public Task<WritingTask?> GetVisibleByIdAsync(Guid tenantId, WritingTaskId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId)));

    public Task<IReadOnlyList<WritingTask>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, int skip, int take, CancellationToken cancellationToken = default)
    {
        IEnumerable<WritingTask> query = Visible(tenantId, track, taskType);
        var items = query.OrderBy(t => t.Title).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<WritingTask>>(items);
    }

    public Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, track, taskType).Count());

    public Task<bool> TitleExistsAsync(Guid? tenantId, string title, WritingTaskId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.Any(t => t.TenantId == tenantId && t.Title == title && (!excludingId.HasValue || t.Id != excludingId.Value)));

    private IEnumerable<WritingTask> Visible(Guid tenantId, ReadingTrack? track, WritingTaskType? taskType) =>
        _tasks
            .Where(t => t.TenantId == null || t.TenantId == tenantId)
            .Where(t => track is null || t.Track == track)
            .Where(t => taskType is null || t.TaskType == taskType);
}
