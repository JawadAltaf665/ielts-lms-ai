using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Application.Tests.SentenceBuilder.Fakes;

internal sealed class FakeSentenceAttemptRepository : ISentenceAttemptRepository
{
    private readonly List<SentenceAttempt> _attempts = [];

    public void Seed(SentenceAttempt attempt) => _attempts.Add(attempt);

    public IReadOnlyList<SentenceAttempt> Attempts => _attempts;

    public Task<SentenceAttempt?> GetByIdAsync(SentenceAttemptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public void Add(SentenceAttempt aggregate) => _attempts.Add(aggregate);

    public void Remove(SentenceAttempt aggregate) => _attempts.Remove(aggregate);
}
