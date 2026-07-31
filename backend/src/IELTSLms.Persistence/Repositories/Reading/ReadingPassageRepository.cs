using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Reading;

internal sealed class ReadingPassageRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ReadingPassage, ReadingPassageId>(dbContext), IReadingPassageRepository
{
    public async Task<ReadingPassage?> GetVisibleByIdAsync(Guid tenantId, ReadingPassageId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(p => p.Id == id && (p.TenantId == null || p.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<ReadingPassage>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, int skip, int take,
        CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, difficulty)
            .OrderBy(p => p.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track, difficulty).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, ReadingPassageId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            p => p.TenantId == tenantId && p.Title == title && (!excludingId.HasValue || p.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    private IQueryable<ReadingPassage> Visible(Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty)
    {
        var query = Set.Where(p => p.TenantId == null || p.TenantId == tenantId);

        if (track is { } trackValue)
        {
            query = query.Where(p => p.Track == trackValue);
        }

        if (difficulty is { } difficultyValue)
        {
            query = query.Where(p => p.Difficulty == difficultyValue);
        }

        return query;
    }
}
