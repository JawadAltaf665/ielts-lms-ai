using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.EventHandlers;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Writing.Events;

namespace IELTSLms.Application.Tests.Writing;

/// <summary>
/// Contract test per Sprint 6 acceptance criteria: reuses the shared <c>IProgressTrackingService</c> —
/// same pattern <c>ReadingAttemptScoredHandlerTests</c>/<c>ListeningAttemptScoredHandlerTests</c> established.
/// </summary>
public sealed class WritingEvaluatedHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Records_one_writing_activity_in_shared_progress_tracking()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new WritingEvaluatedHandler(progressTrackingService, new FakeClock(Now));

        var domainEvent = new WritingEvaluatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 7.0m);

        await handler.Handle(new DomainEventNotification<WritingEvaluatedDomainEvent>(domainEvent), CancellationToken.None);

        var row = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, row.UserId);
        Assert.Equal(LearningSkill.Writing, row.Skill);
        Assert.Equal(1, row.ActivityCount);
    }
}
