using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeSpeakingAttemptRepository : ISpeakingAttemptRepository
{
    private readonly List<SpeakingAttempt> _attempts = [];

    public void Seed(SpeakingAttempt attempt) => _attempts.Add(attempt);

    public IReadOnlyList<SpeakingAttempt> Attempts => _attempts;

    public Task<SpeakingAttempt?> GetByIdAsync(SpeakingAttemptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public void Add(SpeakingAttempt aggregate) => _attempts.Add(aggregate);

    public void Remove(SpeakingAttempt aggregate) => _attempts.Remove(aggregate);
}
