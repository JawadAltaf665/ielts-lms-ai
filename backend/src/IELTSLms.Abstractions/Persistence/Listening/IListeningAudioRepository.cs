using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Abstractions.Persistence.Listening;

/// <summary>
/// Aggregate access for <see cref="ListeningAudio"/> (ERD ENT-024). Not tenant-scoped by the automatic
/// global filter (<see cref="ListeningAudio.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) audios alongside the caller's own tenant-authored ones —
/// mirrors <c>IReadingPassageRepository</c> (Phase 6 Sprint 4).
/// </summary>
public interface IListeningAudioRepository : IRepository<ListeningAudio, ListeningAudioId>
{
    /// <summary>The audio if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<ListeningAudio?> GetVisibleByIdAsync(Guid tenantId, ListeningAudioId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ListeningAudio>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, int skip, int take, CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(Guid tenantId, ReadingTrack? track, CancellationToken cancellationToken = default);

    /// <summary>Whether an audio with this title already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> TitleExistsAsync(Guid? tenantId, string title, ListeningAudioId? excludingId, CancellationToken cancellationToken = default);
}
