using IELTSLms.Domain.Listening;

namespace IELTSLms.Abstractions.Persistence.Listening;

/// <summary>Aggregate access for <see cref="ListeningQuestion"/> (ERD ENT-025).</summary>
public interface IListeningQuestionRepository : IRepository<ListeningQuestion, ListeningQuestionId>
{
    /// <summary>Questions for an audio, ordered by <see cref="ListeningQuestion.Sequence"/>.</summary>
    Task<IReadOnlyList<ListeningQuestion>> ListByAudioAsync(ListeningAudioId audioId, CancellationToken cancellationToken = default);

    Task<bool> SequenceExistsAsync(
        ListeningAudioId audioId, int sequence, ListeningQuestionId? excludingId, CancellationToken cancellationToken = default);
}
