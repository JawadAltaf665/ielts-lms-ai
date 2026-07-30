using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Abstractions.Persistence.Vocabulary;

/// <summary>Aggregate access for <see cref="VocabularyProgress"/> (ERD ENT-018).</summary>
public interface IVocabularyProgressRepository : IRepository<VocabularyProgress, VocabularyProgressId>
{
    Task<VocabularyProgress?> GetByUserAndWordAsync(
        Guid tenantId, Guid userId, VocabularyWordId wordId, CancellationToken cancellationToken = default);

    /// <summary>All of a user's word-progress rows, for the progress-summary endpoint.</summary>
    Task<IReadOnlyList<VocabularyProgress>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Progress rows due on or before <paramref name="asOfUtc"/>, earliest due first — the "due" half of the
    /// next-review-batch query.</summary>
    Task<IReadOnlyList<VocabularyProgress>> ListDueAsync(
        Guid tenantId, Guid userId, DateTimeOffset asOfUtc, int take, CancellationToken cancellationToken = default);
}
