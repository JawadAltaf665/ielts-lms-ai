using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

internal sealed class FakeGrammarTopicRepository : IGrammarTopicRepository
{
    private readonly List<GrammarTopic> _topics = [];

    public void Seed(GrammarTopic topic) => _topics.Add(topic);

    public Task<GrammarTopic?> GetByIdAsync(GrammarTopicId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_topics.FirstOrDefault(t => t.Id == id));

    public void Add(GrammarTopic aggregate) => _topics.Add(aggregate);

    public void Remove(GrammarTopic aggregate) => _topics.Remove(aggregate);

    public Task<GrammarTopic?> GetVisibleByIdAsync(Guid tenantId, GrammarTopicId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_topics.FirstOrDefault(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId)));

    public Task<IReadOnlyList<GrammarTopic>> ListVisibleAsync(
        Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, int skip, int take, CancellationToken cancellationToken = default)
    {
        IEnumerable<GrammarTopic> query = _topics.Where(t => t.TenantId == null || t.TenantId == tenantId);
        if (track is { } trackValue)
        {
            query = query.Where(t => t.Track == trackValue);
        }

        if (difficulty is { } difficultyValue)
        {
            query = query.Where(t => t.Difficulty == difficultyValue);
        }

        IReadOnlyList<GrammarTopic> page = query.OrderBy(t => t.Title).Skip(skip).Take(take).ToArray();
        return Task.FromResult(page);
    }

    public Task<long> CountVisibleAsync(Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, CancellationToken cancellationToken = default)
    {
        IEnumerable<GrammarTopic> query = _topics.Where(t => t.TenantId == null || t.TenantId == tenantId);
        if (track is { } trackValue)
        {
            query = query.Where(t => t.Track == trackValue);
        }

        if (difficulty is { } difficultyValue)
        {
            query = query.Where(t => t.Difficulty == difficultyValue);
        }

        return Task.FromResult((long)query.Count());
    }

    public Task<bool> TitleExistsAsync(Guid? tenantId, string title, GrammarTopicId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_topics.Any(t => t.TenantId == tenantId && t.Title == title && (excludingId is null || t.Id != excludingId.Value)));
}
