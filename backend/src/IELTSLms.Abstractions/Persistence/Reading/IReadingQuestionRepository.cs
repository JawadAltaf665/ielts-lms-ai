using IELTSLms.Domain.Reading;

namespace IELTSLms.Abstractions.Persistence.Reading;

/// <summary>Aggregate access for <see cref="ReadingQuestion"/> (ERD ENT-022).</summary>
public interface IReadingQuestionRepository : IRepository<ReadingQuestion, ReadingQuestionId>
{
    /// <summary>Questions for a passage, ordered by <see cref="ReadingQuestion.Sequence"/>.</summary>
    Task<IReadOnlyList<ReadingQuestion>> ListByPassageAsync(
        ReadingPassageId passageId, CancellationToken cancellationToken = default);

    Task<bool> SequenceExistsAsync(
        ReadingPassageId passageId, int sequence, ReadingQuestionId? excludingId, CancellationToken cancellationToken = default);
}
