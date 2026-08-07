using IELTSLms.Domain.AI;
using IELTSLms.Domain.AI.Events;

namespace IELTSLms.Domain.Tests.AI;

public sealed class LessonSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static LessonSession CreateSession() =>
        LessonSession.Start(
            LessonSessionId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ConversationId.From(Guid.NewGuid()), Now);

    [Fact]
    public void Start_begins_at_Explain()
    {
        var session = CreateSession();

        Assert.Equal(LessonStep.Explain, session.CurrentStep);
        Assert.Equal(0, session.CheckAttempts);
        Assert.Null(session.CompletedAtUtc);
    }

    [Fact]
    public void Advance_cannot_skip_steps_moves_one_at_a_time()
    {
        var session = CreateSession();

        session.Advance(Now);
        Assert.Equal(LessonStep.CheckUnderstanding, session.CurrentStep);
    }

    [Fact]
    public void Advance_on_CheckUnderstanding_is_rejected_must_go_through_RecordCheckAttempt()
    {
        var session = CreateSession();
        session.Advance(Now);
        Assert.Equal(LessonStep.CheckUnderstanding, session.CurrentStep);

        var result = session.Advance(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_LESSON_SESSION_CHECK_STEP_REQUIRES_ATTEMPT", result.Error.Code);
        Assert.Equal(LessonStep.CheckUnderstanding, session.CurrentStep);
    }

    [Fact]
    public void RecordCheckAttempt_failed_does_not_advance_and_repeats_the_step()
    {
        var session = CreateSession();
        session.Advance(Now);

        var result = session.RecordCheckAttempt(passed: false, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(LessonStep.CheckUnderstanding, session.CurrentStep);
        Assert.Equal(1, session.CheckAttempts);
    }

    [Fact]
    public void RecordCheckAttempt_passed_advances_to_Practice()
    {
        var session = CreateSession();
        session.Advance(Now);

        session.RecordCheckAttempt(passed: true, Now);

        Assert.Equal(LessonStep.Practice, session.CurrentStep);
    }

    [Fact]
    public void RecordCheckAttempt_exhausting_the_retry_cap_force_advances_rather_than_looping_forever()
    {
        var session = CreateSession();
        session.Advance(Now);

        session.RecordCheckAttempt(passed: false, Now);
        session.RecordCheckAttempt(passed: false, Now);
        Assert.Equal(LessonStep.CheckUnderstanding, session.CurrentStep);

        session.RecordCheckAttempt(passed: false, Now);

        Assert.Equal(LessonStep.Practice, session.CurrentStep);
        Assert.Equal(3, session.CheckAttempts);
    }

    [Fact]
    public void RecordCheckAttempt_outside_CheckUnderstanding_step_is_rejected()
    {
        var session = CreateSession();

        var result = session.RecordCheckAttempt(passed: true, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_LESSON_SESSION_NOT_AT_CHECK_STEP", result.Error.Code);
        Assert.Equal(LessonStep.Explain, session.CurrentStep);
    }

    [Fact]
    public void Full_sequence_completes_exactly_once_and_raises_LessonSessionCompleted_exactly_once()
    {
        var session = CreateSession();

        session.Advance(Now);
        session.RecordCheckAttempt(passed: true, Now);
        session.Advance(Now);
        session.Advance(Now);

        Assert.Equal(LessonStep.Completed, session.CurrentStep);
        Assert.Equal(Now, session.CompletedAtUtc);
        var completedEvents = session.DomainEvents.OfType<LessonSessionCompletedDomainEvent>().ToList();
        Assert.Single(completedEvents);
        Assert.Equal(session.Id.Value, completedEvents[0].LessonSessionId);
    }

    [Fact]
    public void Advance_after_Completed_is_rejected_idempotent_under_a_retried_request()
    {
        var session = CreateSession();
        session.Advance(Now);
        session.RecordCheckAttempt(passed: true, Now);
        session.Advance(Now);
        session.Advance(Now);
        Assert.Equal(LessonStep.Completed, session.CurrentStep);

        var result = session.Advance(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_LESSON_SESSION_ALREADY_COMPLETED", result.Error.Code);
        Assert.Single(session.DomainEvents.OfType<LessonSessionCompletedDomainEvent>());
    }
}
