using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarTopicRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarTopic, GrammarTopicId>(dbContext), IGrammarTopicRepository
{
    public async Task<GrammarTopic?> GetVisibleByIdAsync(Guid tenantId, GrammarTopicId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.Id == id && (t.TenantId == null || t.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<GrammarTopic>> ListVisibleAsync(
        Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, int skip, int take,
        CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, difficulty)
            .OrderBy(t => t.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(
        Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, difficulty).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, GrammarTopicId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            t => t.TenantId == tenantId && t.Title == title && (!excludingId.HasValue || t.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    private IQueryable<GrammarTopic> Visible(Guid tenantId, GrammarTrack? track, GrammarDifficulty? difficulty)
    {
        var query = Set.Where(t => t.TenantId == null || t.TenantId == tenantId);

        if (track is { } trackValue)
        {
            query = query.Where(t => t.Track == trackValue);
        }

        if (difficulty is { } difficultyValue)
        {
            query = query.Where(t => t.Difficulty == difficultyValue);
        }

        return query;
    }
}
