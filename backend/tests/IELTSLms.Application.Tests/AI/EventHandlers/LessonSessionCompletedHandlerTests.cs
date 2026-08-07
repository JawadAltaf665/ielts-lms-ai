using IELTSLms.Abstractions.Events;
using IELTSLms.Application.AI.EventHandlers;
using IELTSLms.Application.Learning;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI.Events;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.AI.EventHandlers;

/// <summary>Per the sprint's business rule: <c>LessonSessionCompleted</c> routes through the existing
/// <see cref="IProgressTrackingService"/> — the same shared, context-agnostic path
/// <c>GrammarCompletedHandler</c> established, not a parallel progress-update path.</summary>
public sealed class LessonSessionCompletedHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Records_one_ai_teacher_activity_in_shared_progress_tracking()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var progressTrackingService = new ProgressTrackingService(progressTracking, new FakeUnitOfWork(), new FakeGuidProvider());
        var handler = new LessonSessionCompletedHandler(progressTrackingService, new FakeClock(Now));

        var domainEvent = new LessonSessionCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await handler.Handle(new DomainEventNotification<LessonSessionCompletedDomainEvent>(domainEvent), CancellationToken.None);

        var row = Assert.Single(progressTracking.Rows);
        Assert.Equal(domainEvent.UserId, row.UserId);
        Assert.Equal(LearningSkill.AiTeacher, row.Skill);
        Assert.Equal(1, row.ActivityCount);
    }
}
