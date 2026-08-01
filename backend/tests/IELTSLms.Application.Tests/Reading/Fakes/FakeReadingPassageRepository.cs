using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading.Fakes;

internal sealed class FakeReadingPassageRepository : IReadingPassageRepository
{
    private readonly List<ReadingPassage> _passages = [];

    public void Seed(ReadingPassage passage) => _passages.Add(passage);

    public Task<ReadingPassage?> GetByIdAsync(ReadingPassageId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_passages.FirstOrDefault(p => p.Id == id));

    public void Add(ReadingPassage aggregate) => _passages.Add(aggregate);

    public void Remove(ReadingPassage aggregate) => _passages.Remove(aggregate);

    public Task<ReadingPassage?> GetVisibleByIdAsync(Guid tenantId, ReadingPassageId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_passages.FirstOrDefault(p => p.Id == id && (p.TenantId == null || p.TenantId == tenantId)));

    public Task<IReadOnlyList<ReadingPassage>> ListVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, int skip, int take, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ReadingPassage> result = Visible(tenantId, track, difficulty)
            .OrderBy(p => p.Title)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task<long> CountVisibleAsync(
        Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty, CancellationToken cancellationToken = default) =>
        Task.FromResult((long)Visible(tenantId, track, difficulty).Count());

    public Task<bool> TitleExistsAsync(Guid? tenantId, string title, ReadingPassageId? excludingId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_passages.Any(
            p => p.TenantId == tenantId && p.Title == title && (!excludingId.HasValue || p.Id != excludingId.Value)));

    private IEnumerable<ReadingPassage> Visible(Guid tenantId, ReadingTrack? track, ReadingDifficulty? difficulty)
    {
        var query = _passages.Where(p => p.TenantId == null || p.TenantId == tenantId);

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
