using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Domain.Certificates;

namespace IELTSLms.Application.Tests.Certificates.Fakes;

internal sealed class FakeAchievementRepository : IAchievementRepository
{
    private readonly List<Achievement> _achievements = [];

    public IReadOnlyList<Achievement> Achievements => _achievements;

    public Task<Achievement?> GetByIdAsync(AchievementId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_achievements.FirstOrDefault(a => a.Id == id));

    public void Add(Achievement aggregate) => _achievements.Add(aggregate);

    public void Remove(Achievement aggregate) => _achievements.Remove(aggregate);

    public Task<bool> ExistsAsync(Guid tenantId, Guid userId, string achievementType, CancellationToken cancellationToken = default) =>
        Task.FromResult(_achievements.Any(a => a.TenantId == tenantId && a.UserId == userId && a.AchievementType == achievementType));
}
