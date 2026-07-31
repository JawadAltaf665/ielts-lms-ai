using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Application.Tests.Listening.Fakes;

internal sealed class FakeListeningAttemptRepository : IListeningAttemptRepository
{
    private readonly List<ListeningAttempt> _attempts = [];

    public void Seed(ListeningAttempt attempt) => _attempts.Add(attempt);

    public IReadOnlyList<ListeningAttempt> Attempts => _attempts;

    public Task<ListeningAttempt?> GetByIdAsync(ListeningAttemptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public void Add(ListeningAttempt aggregate) => _attempts.Add(aggregate);

    public void Remove(ListeningAttempt aggregate) => _attempts.Remove(aggregate);
}
