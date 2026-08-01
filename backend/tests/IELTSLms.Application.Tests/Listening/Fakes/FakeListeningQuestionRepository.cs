using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Application.Tests.Listening.Fakes;

internal sealed class FakeListeningQuestionRepository : IListeningQuestionRepository
{
    private readonly List<ListeningQuestion> _questions = [];

    public void Seed(ListeningQuestion question) => _questions.Add(question);

    public Task<ListeningQuestion?> GetByIdAsync(ListeningQuestionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.FirstOrDefault(q => q.Id == id));

    public void Add(ListeningQuestion aggregate) => _questions.Add(aggregate);

    public void Remove(ListeningQuestion aggregate) => _questions.Remove(aggregate);

    public Task<IReadOnlyList<ListeningQuestion>> ListByAudioAsync(ListeningAudioId audioId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ListeningQuestion> result = _questions.Where(q => q.ListeningAudioId == audioId).OrderBy(q => q.Sequence).ToArray();
        return Task.FromResult(result);
    }

    public Task<bool> SequenceExistsAsync(
        ListeningAudioId audioId, int sequence, ListeningQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.Any(
            q => q.ListeningAudioId == audioId && q.Sequence == sequence && (excludingId is null || q.Id != excludingId.Value)));
}
