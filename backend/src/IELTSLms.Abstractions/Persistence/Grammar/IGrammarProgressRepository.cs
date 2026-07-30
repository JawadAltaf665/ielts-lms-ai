using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>Aggregate access for <see cref="GrammarProgress"/> (mirrors ENT-018 VocabularyProgress).</summary>
public interface IGrammarProgressRepository : IRepository<GrammarProgress, GrammarProgressId>
{
    Task<GrammarProgress?> GetByUserAndTopicAsync(
        Guid tenantId, Guid userId, GrammarTopicId topicId, CancellationToken cancellationToken = default);

    /// <summary>All of a user's topic-progress rows, for the progress-summary endpoint.</summary>
    Task<IReadOnlyList<GrammarProgress>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
