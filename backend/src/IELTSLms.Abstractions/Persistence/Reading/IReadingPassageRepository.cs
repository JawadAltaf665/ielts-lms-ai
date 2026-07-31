using IELTSLms.Domain.Reading;

namespace IELTSLms.Abstractions.Persistence.Reading;

/// <summary>
/// Aggregate access for <see cref="ReadingPassage"/> (ERD ENT-021). Not tenant-scoped by the automatic
/// global filter (<see cref="ReadingPassage.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) passages alongside the caller's own tenant-authored ones.
/// </summary>
public interface IReadingPassageRepository : IRepository<ReadingPassage, ReadingPassageId>
{
    /// <summary>The passage if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<ReadingPassage?> GetVisibleByIdAsync(Guid tenantId, ReadingPassageId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadingPassage>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, int skip, int take,
        CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, CancellationToken cancellationToken = default);

    /// <summary>Whether a passage with this title already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, ReadingPassageId? excludingId, CancellationToken cancellationToken = default);
}
