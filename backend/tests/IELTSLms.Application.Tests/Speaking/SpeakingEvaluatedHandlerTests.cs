using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Speaking.EventHandlers;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Speaking.Events;

namespace IELTSLms.Application.Tests.Speaking;

/// <summary>Reuses the shared <c>IProgressTrackingService</c> — same pattern as <c>WritingEvaluatedHandlerTests</c>.</summary>
public sealed class SpeakingEvaluatedHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Records_one_speaking_activity_in_shared_progress_tracking()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new SpeakingEvaluatedHandler(progressTrackingService, new FakeClock(Now));

        var domainEvent = new SpeakingEvaluatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 7.0m);

        await handler.Handle(new DomainEventNotification<SpeakingEvaluatedDomainEvent>(domainEvent), CancellationToken.None);

        var row = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, row.UserId);
        Assert.Equal(LearningSkill.Speaking, row.Skill);
        Assert.Equal(1, row.ActivityCount);
    }
}
