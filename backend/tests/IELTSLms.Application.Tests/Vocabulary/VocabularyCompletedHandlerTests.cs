using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Vocabulary.EventHandlers;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Vocabulary.Events;

namespace IELTSLms.Application.Tests.Vocabulary;

/// <summary>
/// Contract test per Sprint-2-Vocabulary-Module.md "Testing": grading a review must publish through the
/// shared cross-module <c>ProgressTracking</c> exactly once, with no parallel progress table introduced.
/// </summary>
public sealed class VocabularyCompletedHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Updates_shared_progress_tracking_exactly_once()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var handler = new VocabularyCompletedHandler(
            new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider()), new FakeClock(Now));
        var domainEvent = new VocabularyCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Grade: 4);

        await handler.Handle(new DomainEventNotification<VocabularyCompletedDomainEvent>(domainEvent), CancellationToken.None);

        var trackingRow = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, trackingRow.UserId);
        Assert.Equal(LearningSkill.Vocabulary, trackingRow.Skill);
        Assert.Equal(1, trackingRow.ActivityCount);
    }

    [Fact]
    public async Task A_second_completed_event_for_the_same_user_updates_the_same_row_not_a_new_one()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var handler = new VocabularyCompletedHandler(
            new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider()), new FakeClock(Now));
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await handler.Handle(
            new DomainEventNotification<VocabularyCompletedDomainEvent>(
                new VocabularyCompletedDomainEvent(tenantId, userId, Guid.NewGuid(), 3)),
            CancellationToken.None);
        await handler.Handle(
            new DomainEventNotification<VocabularyCompletedDomainEvent>(
                new VocabularyCompletedDomainEvent(tenantId, userId, Guid.NewGuid(), 5)),
            CancellationToken.None);

        Assert.Single(progressTracking.Rows);
        Assert.Equal(2, progressTracking.Rows[0].ActivityCount);
    }
}
