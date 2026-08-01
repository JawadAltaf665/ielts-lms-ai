using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Learning.Events;

namespace IELTSLms.Domain.Tests.Learning;

public sealed class ProgressTrackingTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RecordActivity_increments_the_count_and_raises_ProgressRecalculated()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var progress = ProgressTracking.Create(
            ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Grammar, Now);

        progress.RecordActivity(Now.AddMinutes(1));

        Assert.Equal(1, progress.ActivityCount);
        Assert.Equal(Now.AddMinutes(1), progress.LastActivityAtUtc);

        var raised = Assert.Single(progress.DomainEvents);
        var recalculated = Assert.IsType<ProgressRecalculatedDomainEvent>(raised);
        Assert.Equal(tenantId, recalculated.TenantId);
        Assert.Equal(userId, recalculated.UserId);
        Assert.Equal(LearningSkill.Grammar, recalculated.Skill);
    }

    [Fact]
    public void RecordActivity_raises_one_event_per_call()
    {
        var progress = ProgressTracking.Create(
            ProgressTrackingId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), LearningSkill.Writing, Now);

        progress.RecordActivity(Now.AddMinutes(1));
        progress.RecordActivity(Now.AddMinutes(2));

        Assert.Equal(2, progress.ActivityCount);
        Assert.Equal(2, progress.DomainEvents.Count);
    }
}
