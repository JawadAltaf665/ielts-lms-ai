using IELTSLms.Domain.Grammar;

namespace IELTSLms.Abstractions.Persistence.Grammar;

/// <summary>Aggregate access for <see cref="GrammarQuestion"/> (ERD ENT-016, mirrors ENT-022 ReadingQuestions).</summary>
public interface IGrammarQuestionRepository : IRepository<GrammarQuestion, GrammarQuestionId>
{
    /// <summary>Questions for an exercise, ordered by <see cref="GrammarQuestion.Sequence"/>.</summary>
    Task<IReadOnlyList<GrammarQuestion>> ListByExerciseAsync(
        GrammarExerciseId exerciseId, CancellationToken cancellationToken = default);

    Task<bool> SequenceExistsAsync(
        GrammarExerciseId exerciseId, int sequence, GrammarQuestionId? excludingId, CancellationToken cancellationToken = default);
}
