using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>Aggregate access for <see cref="GrammarExercise"/> (ERD ENT-016, drill under a topic).</summary>
public interface IGrammarExerciseRepository : IRepository<GrammarExercise, GrammarExerciseId>
{
    /// <summary>The exercise if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<GrammarExercise?> GetVisibleByIdAsync(Guid tenantId, GrammarExerciseId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrammarExercise>> ListByTopicAsync(
        Guid tenantId, GrammarTopicId topicId, CancellationToken cancellationToken = default);
}
