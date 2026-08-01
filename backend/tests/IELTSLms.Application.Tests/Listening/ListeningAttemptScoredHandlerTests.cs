using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Listening.EventHandlers;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Listening.Events;

namespace IELTSLms.Application.Tests.Listening;

/// <summary>
/// Contract test per Sprint 5 acceptance criteria: reuses the shared <c>IProgressTrackingService</c> —
/// same pattern <c>ReadingAttemptScoredHandlerTests</c> (Phase 6 Sprint 4) established.
/// </summary>
public sealed class ListeningAttemptScoredHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Records_one_listening_activity_in_shared_progress_tracking()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new ListeningAttemptScoredHandler(progressTrackingService, new FakeClock(Now));

        var domainEvent = new ListeningAttemptScoredDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RawScore: 25, MaxScore: 40, Band: 6.5m);

        await handler.Handle(new DomainEventNotification<ListeningAttemptScoredDomainEvent>(domainEvent), CancellationToken.None);

        var row = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, row.UserId);
        Assert.Equal(LearningSkill.Listening, row.Skill);
        Assert.Equal(1, row.ActivityCount);
    }
}
