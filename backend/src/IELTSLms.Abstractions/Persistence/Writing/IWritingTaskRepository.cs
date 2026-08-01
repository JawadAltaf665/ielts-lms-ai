using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>
/// Aggregate access for <see cref="WritingTask"/> (ERD ENT-027). Not tenant-scoped by the automatic
/// global filter (<see cref="WritingTask.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) tasks alongside the caller's own tenant-authored ones —
/// mirrors <c>IReadingPassageRepository</c>/<c>IListeningAudioRepository</c>.
/// </summary>
public interface IWritingTaskRepository : IRepository<WritingTask, WritingTaskId>
{
    /// <summary>The task if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<WritingTask?> GetVisibleByIdAsync(Guid tenantId, WritingTaskId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WritingTask>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, int skip, int take, CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, WritingTaskType? taskType, CancellationToken cancellationToken = default);

    /// <summary>Whether a task with this title already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> TitleExistsAsync(Guid? tenantId, string title, WritingTaskId? excludingId, CancellationToken cancellationToken = default);
}
