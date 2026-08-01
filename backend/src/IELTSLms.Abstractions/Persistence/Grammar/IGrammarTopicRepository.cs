using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>
/// Aggregate access for <see cref="GrammarTopic"/> (ERD ENT-016). Not tenant-scoped by the automatic
/// global filter (<see cref="GrammarTopic.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) topics alongside the caller's own tenant-authored ones.
/// </summary>
public interface IGrammarTopicRepository : IRepository<GrammarTopic, GrammarTopicId>
{
    /// <summary>The topic if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<GrammarTopic?> GetVisibleByIdAsync(Guid tenantId, GrammarTopicId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrammarTopic>> ListVisibleAsync(
        Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, int skip, int take,
        CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(
        Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, CancellationToken cancellationToken = default);

    /// <summary>Whether a topic with this title already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, GrammarTopicId? excludingId, CancellationToken cancellationToken = default);
}
