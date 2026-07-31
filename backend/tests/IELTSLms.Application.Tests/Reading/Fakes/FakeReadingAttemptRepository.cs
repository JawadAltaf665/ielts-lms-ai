using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading.Fakes;

internal sealed class FakeReadingAttemptRepository : IReadingAttemptRepository
{
    private readonly List<ReadingAttempt> _attempts = [];

    public void Seed(ReadingAttempt attempt) => _attempts.Add(attempt);

    public IReadOnlyList<ReadingAttempt> Attempts => _attempts;

    public Task<ReadingAttempt?> GetByIdAsync(ReadingAttemptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public void Add(ReadingAttempt aggregate) => _attempts.Add(aggregate);

    public void Remove(ReadingAttempt aggregate) => _attempts.Remove(aggregate);
}
