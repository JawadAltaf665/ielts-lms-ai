using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Listening.Fakes;

internal sealed class FakeListeningAudioRepository : IListeningAudioRepository
{
    private readonly List<ListeningAudio> _audios = [];

    public void Seed(ListeningAudio audio) => _audios.Add(audio);

    public Task<ListeningAudio?> GetByIdAsync(ListeningAudioId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_audios.FirstOrDefault(a => a.Id == id));

    public void Add(ListeningAudio aggregate) => _audios.Add(aggregate);

    public void Remove(ListeningAudio aggregate) => _audios.Remove(aggregate);

    public Task<ListeningAudio?> GetVisibleByIdAsync(Guid tenantId, ListeningAudioId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_audios.FirstOrDefault(a => a.Id == id && (a.TenantId == null || a.TenantId == tenantId)));

    public Task<IReadOnlyList<ListeningAudio>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, int skip, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ListeningAudio> result = Visible(tenantId, track).OrderBy(a => a.Title).Skip(skip).Take(take).ToArray();
        return Task.FromResult(result);
    }

    public Task<long> CountVisibleAsync(Guid tenantId, ReadingTrack? track, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, track).Count());

    public Task<bool> TitleExistsAsync(Guid? tenantId, string title, ListeningAudioId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_audios.Any(a => a.TenantId == tenantId && a.Title == title && (!excludingId.HasValue || a.Id != excludingId.Value)));

    private IEnumerable<ListeningAudio> Visible(Guid tenantId, ReadingTrack? track)
    {
        var query = _audios.Where(a => a.TenantId == null || a.TenantId == tenantId);
        return track is { } trackValue ? query.Where(a => a.Track == trackValue) : query;
    }
}
