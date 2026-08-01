using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeSpeakingTaskRepository : ISpeakingTaskRepository
{
    private readonly List<SpeakingTask> _tasks = [];

    public void Seed(SpeakingTask task) => _tasks.Add(task);

    public IReadOnlyList<SpeakingTask> Tasks => _tasks;

    public Task<SpeakingTask?> GetByIdAsync(SpeakingTaskId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

    public void Add(SpeakingTask aggregate) => _tasks.Add(aggregate);

    public void Remove(SpeakingTask aggregate) => _tasks.Remove(aggregate);

    public Task<SpeakingTask?> GetVisibleByIdAsync(Guid tenantId, SpeakingTaskId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId)));

    public Task<IReadOnlyList<SpeakingTask>> ListVisibleAsync(
        Guid tenantId, SpeakingPart? part, int skip, int take, CancellationToken cancellationToken = default)
    {
        IEnumerable<SpeakingTask> query = Visible(tenantId, part);
        var items = query.OrderBy(t => t.Title).Skip(skip).Take(take).ToList();
        return Task.FromResult<IReadOnlyList<SpeakingTask>>(items);
    }

    public Task<long> CountVisibleAsync(Guid tenantId, SpeakingPart? part, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, part).Count());

    public Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, SpeakingTaskId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.Any(t => t.TenantId == tenantId && t.Title == title && (!excludingId.HasValue || t.Id != excludingId.Value)));

    private IEnumerable<SpeakingTask> Visible(Guid tenantId, SpeakingPart? part) =>
        _tasks
            .Where(t => t.TenantId == null || t.TenantId == tenantId)
            .Where(t => part is null || t.Part == part);
}
