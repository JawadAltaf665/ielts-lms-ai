using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading.Fakes;

internal sealed class FakeReadingQuestionRepository : IReadingQuestionRepository
{
    private readonly List<ReadingQuestion> _questions = [];

    public void Seed(ReadingQuestion question) => _questions.Add(question);

    public Task<ReadingQuestion?> GetByIdAsync(ReadingQuestionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.FirstOrDefault(q => q.Id == id));

    public void Add(ReadingQuestion aggregate) => _questions.Add(aggregate);

    public void Remove(ReadingQuestion aggregate) => _questions.Remove(aggregate);

    public Task<IReadOnlyList<ReadingQuestion>> ListByPassageAsync(ReadingPassageId passageId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ReadingQuestion> result = _questions
            .Where(q => q.ReadingPassageId == passageId)
            .OrderBy(q => q.Sequence)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task<bool> SequenceExistsAsync(
        ReadingPassageId passageId, int sequence, ReadingQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_questions.Any(
            q => q.ReadingPassageId == passageId && q.Sequence == sequence && (excludingId is null || q.Id != excludingId.Value)));
}
