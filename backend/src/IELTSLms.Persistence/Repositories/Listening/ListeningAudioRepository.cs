using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Listening;

internal sealed class ListeningAudioRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ListeningAudio, ListeningAudioId>(dbContext), IListeningAudioRepository
{
    public async Task<ListeningAudio?> GetVisibleByIdAsync(Guid tenantId, ListeningAudioId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(a => a.Id == id && (a.TenantId == null || a.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<ListeningAudio>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, int skip, int take, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track)
            .OrderBy(a => a.Title)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(Guid tenantId, ReadingTrack? track, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, track).LongCountAsync(cancellationToken).ConfigureAwait(false);

    public async Task<bool> TitleExistsAsync(
        Guid? tenantId, string title, ListeningAudioId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            a => a.TenantId == tenantId && a.Title == title && (!excludingId.HasValue || a.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);

    private IQueryable<ListeningAudio> Visible(Guid tenantId, ReadingTrack? track)
    {
        var query = Set.Where(a => a.TenantId == null || a.TenantId == tenantId);

        if (track is { } trackValue)
        {
            query = query.Where(a => a.Track == trackValue);
        }

        return query;
    }
}
