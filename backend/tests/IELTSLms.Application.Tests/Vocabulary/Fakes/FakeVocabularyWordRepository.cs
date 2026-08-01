using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Tests.Vocabulary.Fakes;

internal sealed class FakeVocabularyWordRepository : IVocabularyWordRepository
{
    private readonly List<VocabularyWord> _words = [];
    private readonly HashSet<VocabularyWordId> _wordsWithProgress = [];

    public void Seed(VocabularyWord word) => _words.Add(word);

    /// <summary>Marks a word as already having a progress row for some user, so <see cref="ListNewForUserAsync"/> excludes it.</summary>
    public void MarkHasProgress(VocabularyWordId wordId) => _wordsWithProgress.Add(wordId);

    public Task<VocabularyWord?> GetByIdAsync(VocabularyWordId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_words.FirstOrDefault(w => w.Id == id));

    public void Add(VocabularyWord aggregate) => _words.Add(aggregate);

    public void Remove(VocabularyWord aggregate) => _words.Remove(aggregate);

    public Task<VocabularyWord?> GetVisibleByIdAsync(Guid tenantId, VocabularyWordId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_words.FirstOrDefault(w => w.Id == id && (w.TenantId == null || w.TenantId == tenantId)));

    public Task<IReadOnlyList<VocabularyWord>> ListVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VocabularyWord> page = Visible(tenantId, topic, difficultyLevel, search).OrderBy(w => w.Word).Skip(skip).Take(take).ToArray();
        return Task.FromResult(page);
    }

    public Task<long> CountVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, topic, difficultyLevel, search).Count());

    public Task<bool> WordSenseExistsAsync(
        Guid? tenantId, string word, string sense, VocabularyWordId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_words.Any(
            w => w.TenantId == tenantId && w.Word == word && w.Sense == sense && (excludingId is null || w.Id != excludingId.Value)));

    public Task<IReadOnlyList<VocabularyWord>> GetByIdsAsync(
        IReadOnlyCollection<VocabularyWordId> ids, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VocabularyWord> result = _words.Where(w => ids.Contains(w.Id)).ToArray();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<VocabularyWord>> ListNewForUserAsync(
        Guid tenantId, Guid userId, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VocabularyWord> result = _words
            .Where(w => w.TenantId == null || w.TenantId == tenantId)
            .Where(w => !_wordsWithProgress.Contains(w.Id))
            .OrderBy(w => w.Word)
            .Take(take)
            .ToArray();
        return Task.FromResult(result);
    }

    private IEnumerable<VocabularyWord> Visible(Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search)
    {
        IEnumerable<VocabularyWord> query = _words.Where(w => w.TenantId == null || w.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query.Where(w => w.Topic == topic);
        }

        if (difficultyLevel is { } difficultyLevelValue)
        {
            query = query.Where(w => w.DifficultyLevel == difficultyLevelValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(w =>
                w.Word.Contains(search, StringComparison.OrdinalIgnoreCase) || w.Meaning.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
