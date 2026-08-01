using IELTSLms.Abstractions.Events;
using IELTSLms.Application.Grammar.EventHandlers;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.Grammar.Events;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Grammar;

/// <summary>
/// Contract test per Sprint-1-Grammar-Module.md "Testing": firing <c>GrammarCompleted</c> must update
/// both Grammar's own <c>GrammarProgress</c> and the shared cross-module <c>ProgressTracking</c>
/// exactly once.
/// </summary>
public sealed class GrammarCompletedHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static GrammarCompletedHandler CreateHandler(
        FakeGrammarProgressRepository grammarProgress, FakeProgressTrackingRepository progressTracking)
    {
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        return new GrammarCompletedHandler(
            grammarProgress, new FakeUnitOfWork(), progressTrackingService, new FakeClock(Now), new FakeGuidProvider());
    }

    [Fact]
    public async Task Updates_grammar_progress_and_shared_progress_tracking_exactly_once()
    {
        var grammarProgress = new FakeGrammarProgressRepository();
        var progressTracking = new FakeProgressTrackingRepository();
        var handler = CreateHandler(grammarProgress, progressTracking);

        var domainEvent = new GrammarCompletedDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RawScore: 4, MaxScore: 5);

        await handler.Handle(new DomainEventNotification<GrammarCompletedDomainEvent>(domainEvent), CancellationToken.None);

        var progressRow = Assert.Single(grammarProgress.Progress);
        Assert.Equal(domainEvent.UserId, progressRow.UserId);
        Assert.Equal(domainEvent.GrammarTopicId, progressRow.GrammarTopicId.Value);
        Assert.Equal(1, progressRow.AttemptCount);

        var trackingRow = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, trackingRow.UserId);
        Assert.Equal(LearningSkill.Grammar, trackingRow.Skill);
        Assert.Equal(1, trackingRow.ActivityCount);
    }

    [Fact]
    public async Task A_second_completed_event_for_the_same_topic_updates_the_same_rows_not_new_ones()
    {
        var grammarProgress = new FakeGrammarProgressRepository();
        var progressTracking = new FakeProgressTrackingRepository();
        var handler = CreateHandler(grammarProgress, progressTracking);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var topicId = Guid.NewGuid();

        await handler.Handle(
            new DomainEventNotification<GrammarCompletedDomainEvent>(
                new GrammarCompletedDomainEvent(tenantId, userId, topicId, Guid.NewGuid(), Guid.NewGuid(), 3, 5)),
            CancellationToken.None);
        await handler.Handle(
            new DomainEventNotification<GrammarCompletedDomainEvent>(
                new GrammarCompletedDomainEvent(tenantId, userId, topicId, Guid.NewGuid(), Guid.NewGuid(), 5, 5)),
            CancellationToken.None);

        Assert.Single(grammarProgress.Progress);
        Assert.Equal(2, grammarProgress.Progress[0].AttemptCount);

        Assert.Single(progressTracking.Rows);
        Assert.Equal(2, progressTracking.Rows[0].ActivityCount);
    }
}
