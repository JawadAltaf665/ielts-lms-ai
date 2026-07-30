using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Grammar.Fakes;

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
}
