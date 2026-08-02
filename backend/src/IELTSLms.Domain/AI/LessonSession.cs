using IELTSLms.Domain.AI.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// The AI Teacher's step-state machine for one learner/lesson pairing (Phase 7 Sprint 4, Agentic AI
/// Architecture §12.1 — new "Teacher Agent" entry, distinct from §12.4's human-teacher-facing "Teacher
/// Assistant Agent"). Backs a <see cref="Conversation"/> (<see cref="ConversationId"/>) scoped to one
/// lesson rather than the open-ended thread <c>Conversation</c> supports on its own.
/// <para>
/// <see cref="LessonId"/> is a bare id, not a foreign key: the Course/Module/Lesson content model
/// (ERD ENT-014) does not exist yet in this codebase, so there is nothing to reference — mirrors the
/// documented "column now, FK later" precedent (<c>CLAUDE.md</c> Sprint 2 identity tables). Reconcile
/// once Lessons ships.
/// </para>
/// </summary>
public sealed class LessonSession : TenantAuditableEntity<LessonSessionId>
{
    /// <summary>Loop-safety cap on <see cref="LessonStep.CheckUnderstanding"/> retries (Agentic AI
    /// Architecture §7). ponytail: exceeding the cap force-advances instead of introducing a distinct
    /// "stuck"/escalation terminal state — the sprint scope is the happy-path state machine, not a
    /// human-escalation workflow; add a genuine escalation status if repeated cap-outs turn out to be
    /// common in practice.</summary>
    private const int MaxCheckAttempts = 3;

    private LessonSession(
        LessonSessionId id, Guid tenantId, Guid userId, Guid lessonId, ConversationId conversationId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        LessonId = lessonId;
        ConversationId = conversationId;
        CurrentStep = LessonStep.Explain;
        CheckAttempts = 0;
        StartedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private LessonSession()
    {
    }

    public Guid UserId { get; private init; }

    /// <summary>See the type-level remarks — no FK until a Lessons module exists.</summary>
    public Guid LessonId { get; private init; }

    public ConversationId ConversationId { get; private init; }

    public LessonStep CurrentStep { get; private set; }

    /// <summary>Failed <see cref="LessonStep.CheckUnderstanding"/> attempts in the current pass; reset
    /// implicitly by never being touched again once the step advances.</summary>
    public int CheckAttempts { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private init; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static LessonSession Start(
        LessonSessionId id, Guid tenantId, Guid userId, Guid lessonId, ConversationId conversationId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, lessonId, conversationId, nowUtc);

    /// <summary>Unconditionally moves to the next step in the fixed sequence (Explain → Check
    /// Understanding → Practice → Recap → Completed), raising
    /// <see cref="LessonSessionCompletedDomainEvent"/> exactly once when it reaches
    /// <see cref="LessonStep.Completed"/>. Not valid for <see cref="LessonStep.CheckUnderstanding"/> —
    /// that step must go through <see cref="RecordCheckAttempt"/> so a failed check cannot advance it.</summary>
    public Result Advance(DateTimeOffset nowUtc)
    {
        if (CurrentStep == LessonStep.Completed)
        {
            return Result.Failure(LessonSessionDomainErrors.AlreadyCompleted);
        }

        if (CurrentStep == LessonStep.CheckUnderstanding)
        {
            return Result.Failure(LessonSessionDomainErrors.CheckStepRequiresRecordedAttempt);
        }

        return AdvanceCore(nowUtc);
    }

    /// <summary>Records one <see cref="LessonStep.CheckUnderstanding"/> attempt. Advances only on a pass
    /// or once <see cref="MaxCheckAttempts"/> is reached (loop-safety cap); a failed attempt under the
    /// cap repeats the same step — the step-advance invariant the sprint's state-machine tests assert.</summary>
    public Result RecordCheckAttempt(bool passed, DateTimeOffset nowUtc)
    {
        if (CurrentStep != LessonStep.CheckUnderstanding)
        {
            return Result.Failure(LessonSessionDomainErrors.NotAtCheckUnderstandingStep);
        }

        CheckAttempts++;
        UpdatedAtUtc = nowUtc;

        if (passed || CheckAttempts >= MaxCheckAttempts)
        {
            return AdvanceCore(nowUtc);
        }

        return Result.Success();
    }

    private Result AdvanceCore(DateTimeOffset nowUtc)
    {
        var next = CurrentStep switch
        {
            LessonStep.Explain => LessonStep.CheckUnderstanding,
            LessonStep.CheckUnderstanding => LessonStep.Practice,
            LessonStep.Practice => LessonStep.Recap,
            LessonStep.Recap => LessonStep.Completed,
            _ => LessonStep.Completed,
        };

        CurrentStep = next;
        UpdatedAtUtc = nowUtc;

        if (next == LessonStep.Completed)
        {
            CompletedAtUtc = nowUtc;
            RaiseDomainEvent(new LessonSessionCompletedDomainEvent(TenantId, UserId, LessonId, Id.Value));
        }

        return Result.Success();
    }
}

internal static class LessonSessionDomainErrors
{
    public static readonly Error AlreadyCompleted = Error.Validation(
        "BUSINESS_LESSON_SESSION_ALREADY_COMPLETED", "This lesson session has already been completed.");

    public static readonly Error NotAtCheckUnderstandingStep = Error.Validation(
        "BUSINESS_LESSON_SESSION_NOT_AT_CHECK_STEP", "The session is not currently at the check-understanding step.");

    public static readonly Error CheckStepRequiresRecordedAttempt = Error.Validation(
        "BUSINESS_LESSON_SESSION_CHECK_STEP_REQUIRES_ATTEMPT",
        "The check-understanding step must advance through a recorded attempt, not an unconditional advance.");
}
