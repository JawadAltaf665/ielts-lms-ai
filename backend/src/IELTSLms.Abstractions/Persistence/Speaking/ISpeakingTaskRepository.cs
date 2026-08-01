using IELTSLms.Domain.Speaking;

namespace IELTSLms.Abstractions.Persistence.Speaking;

/// <summary>
/// Aggregate access for <see cref="SpeakingTask"/> (ERD ENT-030). Not tenant-scoped by the automatic
/// global filter (<see cref="SpeakingTask.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) tasks alongside the caller's own tenant-authored ones —
/// mirrors <c>IWritingTaskRepository</c>.
/// </summary>
public interface ISpeakingTaskRepository : IRepository<SpeakingTask, SpeakingTaskId>
{
    /// <summary>The task if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<SpeakingTask?> GetVisibleByIdAsync(Guid tenantId, SpeakingTaskId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpeakingTask>> ListVisibleAsync(
        Guid tenantId, SpeakingPart? part, int skip, int take, CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(Guid tenantId, SpeakingPart? part, CancellationToken cancellationToken = default);

    /// <summary>Whether a task with this title already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> TitleExistsAsync(Guid? tenantId, string title, SpeakingTaskId? excludingId, CancellationToken cancellationToken = default);
}
