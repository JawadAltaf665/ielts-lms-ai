using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Reading.EventHandlers;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Reading.Events;

namespace IELTSLms.Application.Tests.Reading;

/// <summary>
/// Contract test per Sprint 4 acceptance criteria: "Reuses Sprint 1's IProgressTrackingService — no
/// parallel progress table." Firing <c>ReadingAttemptScored</c> must update the shared cross-module
/// <c>ProgressTracking</c> under <see cref="LearningSkill.Reading"/>.
/// </summary>
public sealed class ReadingAttemptScoredHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Records_one_reading_activity_in_shared_progress_tracking()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new ReadingAttemptScoredHandler(progressTrackingService, new FakeClock(Now));

        var domainEvent = new ReadingAttemptScoredDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RawScore: 30, MaxScore: 40, Band: 7.0m);

        await handler.Handle(new DomainEventNotification<ReadingAttemptScoredDomainEvent>(domainEvent), CancellationToken.None);

        var row = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, row.UserId);
        Assert.Equal(LearningSkill.Reading, row.Skill);
        Assert.Equal(1, row.ActivityCount);
    }

    [Fact]
    public async Task A_second_scored_event_for_the_same_user_updates_the_same_row_not_a_new_one()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new ReadingAttemptScoredHandler(progressTrackingService, new FakeClock(Now));
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await handler.Handle(
            new DomainEventNotification<ReadingAttemptScoredDomainEvent>(
                new ReadingAttemptScoredDomainEvent(tenantId, userId, Guid.NewGuid(), Guid.NewGuid(), 20, 40, 5.0m)),
            CancellationToken.None);
        await handler.Handle(
            new DomainEventNotification<ReadingAttemptScoredDomainEvent>(
                new ReadingAttemptScoredDomainEvent(tenantId, userId, Guid.NewGuid(), Guid.NewGuid(), 35, 40, 8.0m)),
            CancellationToken.None);

        Assert.Single(progressTracking.Rows);
        Assert.Equal(2, progressTracking.Rows[0].ActivityCount);
    }
}
