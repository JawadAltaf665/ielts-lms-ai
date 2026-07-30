using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

internal sealed class FakeGrammarAttemptRepository : IGrammarAttemptRepository
{
    private readonly List<GrammarAttempt> _attempts = [];

    public void Seed(GrammarAttempt attempt) => _attempts.Add(attempt);

    public IReadOnlyList<GrammarAttempt> Attempts => _attempts;

    public Task<GrammarAttempt?> GetByIdAsync(GrammarAttemptId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_attempts.FirstOrDefault(a => a.Id == id));

    public void Add(GrammarAttempt aggregate) => _attempts.Add(aggregate);

    public void Remove(GrammarAttempt aggregate) => _attempts.Remove(aggregate);
}
