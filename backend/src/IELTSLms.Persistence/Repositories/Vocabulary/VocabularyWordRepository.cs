using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Vocabulary;

internal sealed class VocabularyWordRepository(ApplicationDbContext dbContext)
    : RepositoryBase<VocabularyWord, VocabularyWordId>(dbContext), IVocabularyWordRepository
{
    public async Task<VocabularyWord?> GetVisibleByIdAsync(Guid tenantId, VocabularyWordId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(w => w.Id == id && (w.TenantId == null || w.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<VocabularyWord>> ListVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search, int skip, int take,
        CancellationToken cancellationToken = default) =>
        await Visible(tenantId, topic, difficultyLevel, search)
            .OrderBy(w => w.Word)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(
        Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search,
        CancellationToken cancellationToken = default) =>
        await Visible(tenantId, topic, difficultyLevel, search).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> WordSenseExistsAsync(
        Guid? tenantId, string word, string sense, VocabularyWordId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            w => w.TenantId == tenantId && w.Word == word && w.Sense == sense && (!excludingId.HasValue || w.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<VocabularyWord>> GetByIdsAsync(
        IReadOnlyCollection<VocabularyWordId> ids, CancellationToken cancellationToken = default) =>
        await Set.Where(w => ids.Contains(w.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<VocabularyWord>> ListNewForUserAsync(
        Guid tenantId, Guid userId, int take, CancellationToken cancellationToken = default) =>
        await Set
            .Where(w => w.TenantId == null || w.TenantId == tenantId)
            .Where(w => !DbContext.Set<VocabularyProgress>().Any(p => p.UserId == userId && p.VocabularyWordId == w.Id))
            .OrderBy(w => w.Word)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private IQueryable<VocabularyWord> Visible(Guid tenantId, string? topic, VocabularyDifficultyLevel? difficultyLevel, string? search)
    {
        var query = Set.Where(w => w.TenantId == null || w.TenantId == tenantId);

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
            query = query.Where(w => EF.Functions.ILike(w.Word, $"%{search}%") || EF.Functions.ILike(w.Meaning, $"%{search}%"));
        }

        return query;
    }
}
