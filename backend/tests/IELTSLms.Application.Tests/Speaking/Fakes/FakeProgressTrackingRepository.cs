using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeProgressTrackingRepository : IProgressTrackingRepository
{
    private readonly List<ProgressTracking> _rows = [];

    public IReadOnlyList<ProgressTracking> Rows => _rows;

    public Task<ProgressTracking?> GetByIdAsync(ProgressTrackingId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_rows.FirstOrDefault(r => r.Id == id));

    public void Add(ProgressTracking aggregate) => _rows.Add(aggregate);

    public void Remove(ProgressTracking aggregate) => _rows.Remove(aggregate);

    public Task<ProgressTracking?> GetByUserAndSkillAsync(
        Guid tenantId, Guid userId, LearningSkill skill, CancellationToken cancellationToken = default) =>
        Task.FromResult(_rows.FirstOrDefault(r => r.TenantId == tenantId && r.UserId == userId && r.Skill == skill));

    public Task<IReadOnlyList<ProgressTracking>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProgressTracking>>(
            _rows.Where(r => r.TenantId == tenantId && r.UserId == userId).ToList());

    public Task<IReadOnlyList<Guid>> ListActiveUserIdsAsync(
        Guid tenantId, DateTimeOffset activeSinceUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>(
            _rows.Where(r => r.TenantId == tenantId && r.LastActivityAtUtc >= activeSinceUtc)
                .Select(r => r.UserId).Distinct().ToList());
}
