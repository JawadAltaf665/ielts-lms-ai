using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

internal sealed class FakeGrammarProgressRepository : IGrammarProgressRepository
{
    private readonly List<GrammarProgress> _progress = [];

    public IReadOnlyList<GrammarProgress> Progress => _progress;

    public Task<GrammarProgress?> GetByIdAsync(GrammarProgressId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_progress.FirstOrDefault(p => p.Id == id));

    public void Add(GrammarProgress aggregate) => _progress.Add(aggregate);

    public void Remove(GrammarProgress aggregate) => _progress.Remove(aggregate);

    public Task<GrammarProgress?> GetByUserAndTopicAsync(
        Guid tenantId, Guid userId, GrammarTopicId topicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_progress.FirstOrDefault(p => p.TenantId == tenantId && p.UserId == userId && p.GrammarTopicId == topicId));

    public Task<IReadOnlyList<GrammarProgress>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GrammarProgress> result = _progress.Where(p => p.TenantId == tenantId && p.UserId == userId).ToArray();
        return Task.FromResult(result);
    }
}
