using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Tests.Vocabulary.Fakes;

internal sealed class FakeVocabularyProgressRepository : IVocabularyProgressRepository
{
    private readonly List<VocabularyProgress> _progress = [];

    public IReadOnlyList<VocabularyProgress> Progress => _progress;

    public void Seed(VocabularyProgress progress) => _progress.Add(progress);

    public Task<VocabularyProgress?> GetByIdAsync(VocabularyProgressId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_progress.FirstOrDefault(p => p.Id == id));

    public void Add(VocabularyProgress aggregate) => _progress.Add(aggregate);

    public void Remove(VocabularyProgress aggregate) => _progress.Remove(aggregate);

    public Task<VocabularyProgress?> GetByUserAndWordAsync(
        Guid tenantId, Guid userId, VocabularyWordId wordId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_progress.FirstOrDefault(p => p.TenantId == tenantId && p.UserId == userId && p.VocabularyWordId == wordId));

    public Task<IReadOnlyList<VocabularyProgress>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VocabularyProgress> result = _progress.Where(p => p.TenantId == tenantId && p.UserId == userId).ToArray();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<VocabularyProgress>> ListDueAsync(
        Guid tenantId, Guid userId, DateTimeOffset asOfUtc, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VocabularyProgress> result = _progress
            .Where(p => p.TenantId == tenantId && p.UserId == userId && p.NextReviewAtUtc != null && p.NextReviewAtUtc <= asOfUtc)
            .OrderBy(p => p.NextReviewAtUtc)
            .Take(take)
            .ToArray();
        return Task.FromResult(result);
    }
}
