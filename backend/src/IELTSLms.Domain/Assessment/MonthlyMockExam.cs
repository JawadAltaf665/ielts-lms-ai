using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Assessment;

/// <summary>
/// A learner's single timed session stitching all four skills together (ERD ENT-036, DDD §13.5). Per
/// AR-2 ("reference other aggregates by identity only"), the four section attempts
/// (<see cref="ReadingAttemptId"/>/<see cref="ListeningAttemptId"/>/<see cref="WritingSubmissionId"/>/
/// <see cref="SpeakingAttemptId"/>) are stored as plain <see cref="Guid"/> references — never object
/// navigation into the Reading/Listening/Writing/Speaking aggregates.
///
/// Reading/Listening already support a decoupled Start-then-Submit flow in their own modules, so
/// <see cref="StartSection"/> captures their attempt id immediately (the caller delegates to their
/// existing <c>Start&lt;Skill&gt;Attempt</c> command first). Writing/Speaking have no such phase (their
/// own <c>Create</c>/<c>Submit</c> is atomic) — for those two, <see cref="StartSection"/> only stamps the
/// server timer, and <see cref="LinkSectionAttempt"/> attaches the resulting submission id once the
/// candidate submits via the skill's own standalone endpoint (matched to this exam via the "single
/// active session per user" invariant, since a user can have at most one <see cref="MockExamState.InProgress"/>
/// exam at a time — see <c>Application/Assessment/EventHandlers</c>).
/// </summary>
public sealed class MonthlyMockExam : TenantAuditableEntity<MockExamId>
{
    private MonthlyMockExam(MockExamId id, Guid tenantId, Guid userId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        State = MockExamState.InProgress;
        StartedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private MonthlyMockExam()
    {
    }

    public Guid UserId { get; private init; }

    public MockExamState State { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private init; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? AbandonedAtUtc { get; private set; }

    public Guid? ReadingAttemptId { get; private set; }
    public DateTimeOffset? ReadingStartedAtUtc { get; private set; }
    public DateTimeOffset? ReadingDeadlineAtUtc { get; private set; }
    public DateTimeOffset? ReadingCompletedAtUtc { get; private set; }

    public Guid? ListeningAttemptId { get; private set; }
    public DateTimeOffset? ListeningStartedAtUtc { get; private set; }
    public DateTimeOffset? ListeningDeadlineAtUtc { get; private set; }
    public DateTimeOffset? ListeningCompletedAtUtc { get; private set; }

    public Guid? WritingSubmissionId { get; private set; }
    public DateTimeOffset? WritingStartedAtUtc { get; private set; }
    public DateTimeOffset? WritingDeadlineAtUtc { get; private set; }
    public DateTimeOffset? WritingCompletedAtUtc { get; private set; }

    public Guid? SpeakingAttemptId { get; private set; }
    public DateTimeOffset? SpeakingStartedAtUtc { get; private set; }
    public DateTimeOffset? SpeakingDeadlineAtUtc { get; private set; }
    public DateTimeOffset? SpeakingCompletedAtUtc { get; private set; }

    public bool AllSectionsCompleted =>
        ReadingCompletedAtUtc is not null && ListeningCompletedAtUtc is not null
        && WritingCompletedAtUtc is not null && SpeakingCompletedAtUtc is not null;

    public static MonthlyMockExam Start(MockExamId id, Guid tenantId, Guid userId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, nowUtc);

    public DateTimeOffset? SectionStartedAt(MockExamSectionType section) => section switch
    {
        MockExamSectionType.Reading => ReadingStartedAtUtc,
        MockExamSectionType.Listening => ListeningStartedAtUtc,
        MockExamSectionType.Writing => WritingStartedAtUtc,
        MockExamSectionType.Speaking => SpeakingStartedAtUtc,
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
    };

    public Guid? SectionAttemptId(MockExamSectionType section) => section switch
    {
        MockExamSectionType.Reading => ReadingAttemptId,
        MockExamSectionType.Listening => ListeningAttemptId,
        MockExamSectionType.Writing => WritingSubmissionId,
        MockExamSectionType.Speaking => SpeakingAttemptId,
        _ => throw new ArgumentOutOfRangeException(nameof(section)),
    };

    /// <summary>Stamps the server-owned start/deadline for a section. Idempotency guard: refuses a second start of the same section.</summary>
    public Result StartSection(
        MockExamSectionType section, Guid? attemptId, DateTimeOffset startedAtUtc, DateTimeOffset deadlineAtUtc)
    {
        if (State != MockExamState.InProgress)
        {
            return Result.Failure(MockExamDomainErrors.NotInProgress);
        }

        if (SectionStartedAt(section) is not null)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_MOCK_EXAM_SECTION_ALREADY_STARTED", $"The {section} section has already been started."));
        }

        switch (section)
        {
            case MockExamSectionType.Reading:
                ReadingAttemptId = attemptId;
                ReadingStartedAtUtc = startedAtUtc;
                ReadingDeadlineAtUtc = deadlineAtUtc;
                break;
            case MockExamSectionType.Listening:
                ListeningAttemptId = attemptId;
                ListeningStartedAtUtc = startedAtUtc;
                ListeningDeadlineAtUtc = deadlineAtUtc;
                break;
            case MockExamSectionType.Writing:
                WritingStartedAtUtc = startedAtUtc;
                WritingDeadlineAtUtc = deadlineAtUtc;
                break;
            case MockExamSectionType.Speaking:
                SpeakingStartedAtUtc = startedAtUtc;
                SpeakingDeadlineAtUtc = deadlineAtUtc;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(section));
        }

        UpdatedAtUtc = startedAtUtc;
        return Result.Success();
    }

    /// <summary>
    /// Attaches the Writing/Speaking submission id discovered after the fact, via the submitted-event
    /// handler. A no-op (idempotent) if the section is already linked to this exact attempt.
    /// </summary>
    public Result LinkSectionAttempt(MockExamSectionType section, Guid attemptId, DateTimeOffset nowUtc)
    {
        if (State != MockExamState.InProgress)
        {
            return Result.Failure(MockExamDomainErrors.NotInProgress);
        }

        if (section is not (MockExamSectionType.Writing or MockExamSectionType.Speaking))
        {
            return Result.Failure(Error.Failure(
                "GENERAL_MOCK_EXAM_SECTION_NOT_LINKABLE", "Only Writing and Speaking sections are linked after the fact."));
        }

        if (SectionStartedAt(section) is null)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_MOCK_EXAM_SECTION_NOT_STARTED", $"The {section} section has not been started."));
        }

        if (SectionAttemptId(section) == attemptId)
        {
            return Result.Success();
        }

        if (section == MockExamSectionType.Writing)
        {
            WritingSubmissionId = attemptId;
        }
        else
        {
            SpeakingAttemptId = attemptId;
        }

        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>Marks a section evaluated. Idempotent: a section already marked completed succeeds without re-stamping.</summary>
    public Result CompleteSection(MockExamSectionType section, DateTimeOffset nowUtc)
    {
        if (State != MockExamState.InProgress)
        {
            return Result.Failure(MockExamDomainErrors.NotInProgress);
        }

        switch (section)
        {
            case MockExamSectionType.Reading:
                ReadingCompletedAtUtc ??= nowUtc;
                break;
            case MockExamSectionType.Listening:
                ListeningCompletedAtUtc ??= nowUtc;
                break;
            case MockExamSectionType.Writing:
                WritingCompletedAtUtc ??= nowUtc;
                break;
            case MockExamSectionType.Speaking:
                SpeakingCompletedAtUtc ??= nowUtc;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(section));
        }

        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>Finalizes the session once every section is evaluated and raises <see cref="Events.MockExamCompletedDomainEvent"/>.</summary>
    public Result Complete(decimal overallBand, DateTimeOffset nowUtc)
    {
        if (State != MockExamState.InProgress)
        {
            return Result.Failure(MockExamDomainErrors.NotInProgress);
        }

        if (!AllSectionsCompleted)
        {
            return Result.Failure(MockExamDomainErrors.SectionsIncomplete);
        }

        State = MockExamState.Completed;
        CompletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new Events.MockExamCompletedDomainEvent(TenantId, UserId, Id.Value, overallBand));
        return Result.Success();
    }

    /// <summary>Abandons an in-progress session, releasing the single-active-session lock.</summary>
    public Result Abandon(DateTimeOffset nowUtc)
    {
        if (State != MockExamState.InProgress)
        {
            return Result.Failure(MockExamDomainErrors.NotInProgress);
        }

        State = MockExamState.Abandoned;
        AbandonedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class MockExamDomainErrors
{
    public static readonly Error NotInProgress = Error.Conflict(
        "CONFLICT_MOCK_EXAM_NOT_IN_PROGRESS", "This mock exam session is not in progress.");

    public static readonly Error SectionsIncomplete = Error.Conflict(
        "BUSINESS_MOCK_EXAM_SECTIONS_INCOMPLETE", "Not all sections have been evaluated yet.");
}
