using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Abstractions.Persistence.Vocabulary;

/// <summary>
/// Aggregate access for <see cref="VocabularyWord"/> (ERD ENT-017). Not tenant-scoped by the automatic
/// global filter (<see cref="VocabularyWord.TenantId"/> is nullable), so every read here explicitly
/// includes global (<c>TenantId == null</c>) words alongside the caller's own tenant-authored ones.
/// </summary>
public interface IVocabularyWordRepository : IRepository<VocabularyWord, VocabularyWordId>
{
    /// <summary>The word if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<VocabularyWord?> GetVisibleByIdAsync(Guid tenantId, VocabularyWordId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VocabularyWord>> ListVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search, int skip, int take,
        CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search,
        CancellationToken cancellationToken = default);

    /// <summary>Whether a word with this (word, sense) pair already exists in the same scope (global, or the given tenant).</summary>
    Task<bool> WordSenseExistsAsync(
        Guid? tenantId, string word, string sense, VocabularyWordId? excludingId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VocabularyWord>> GetByIdsAsync(
        IReadOnlyCollection<VocabularyWordId> ids, CancellationToken cancellationToken = default);

    /// <summary>Words visible to the tenant that <paramref name="userId"/> has no <see cref="VocabularyProgress"/> row
    /// for yet — the "new" fill-in half of the next-review-batch query.</summary>
    Task<IReadOnlyList<VocabularyWord>> ListNewForUserAsync(
        Guid tenantId, Guid userId, int take, CancellationToken cancellationToken = default);
}
