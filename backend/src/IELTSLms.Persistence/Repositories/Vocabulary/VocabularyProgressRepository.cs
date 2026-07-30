using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Vocabulary;

internal sealed class VocabularyProgressRepository(ApplicationDbContext dbContext)
    : RepositoryBase<VocabularyProgress, VocabularyProgressId>(dbContext), IVocabularyProgressRepository
{
    public async Task<VocabularyProgress?> GetByUserAndWordAsync(
        Guid tenantId, Guid userId, VocabularyWordId wordId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.UserId == userId && p.VocabularyWordId == wordId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<VocabularyProgress>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<VocabularyProgress>> ListDueAsync(
        Guid tenantId, Guid userId, DateTimeOffset asOfUtc, int take, CancellationToken cancellationToken = default) =>
        await Set
            .Where(p => p.TenantId == tenantId && p.UserId == userId && p.NextReviewAtUtc != null && p.NextReviewAtUtc <= asOfUtc)
            .OrderBy(p => p.NextReviewAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
