using IELTSLms.Application.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.Fakes;

internal sealed class FakeProgressTrackingService : IProgressTrackingService
{
    public List<(Guid TenantId, Guid UserId, LearningSkill Skill)> RecordedActivities { get; } = [];

    public Task RecordSkillActivityAsync(
        Guid tenantId, Guid userId, LearningSkill skill, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        RecordedActivities.Add((tenantId, userId, skill));
        return Task.CompletedTask;
    }
}
