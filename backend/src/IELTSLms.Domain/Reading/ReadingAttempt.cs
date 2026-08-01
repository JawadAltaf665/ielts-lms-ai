using IELTSLms.Domain.Reading.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Reading;

/// <summary>
/// A learner's timed attempt at a <see cref="ReadingPassage"/> (ERD ENT-023). Unlike
/// <c>GrammarAttempt</c> (scored in one shot), a reading attempt has two phases: <see cref="Start"/>
/// stamps the server-owned deadline before any answers exist, and <see cref="Submit"/> grades and
/// closes it later — the template Sprint 5 (Listening) and Sprint 8 (Mock Exam) timers reuse.
/// <see cref="StudySessionId"/>/<see cref="WeeklyAssessmentId"/>/<see cref="MockExamId"/> stay
/// <see langword="null"/> this sprint; later sprints populate them when composing a passage into a
/// session or exam.
/// </summary>
public sealed class ReadingAttempt : TenantAuditableEntity<ReadingAttemptId>
{
    private ReadingAttempt(
        ReadingAttemptId id, Guid tenantId, Guid userId, ReadingPassageId passageId, DateTimeOffset startedAtUtc,
        DateTimeOffset deadlineAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        ReadingPassageId = passageId;
        StartedAtUtc = startedAtUtc;
        DeadlineAtUtc = deadlineAtUtc;
        CreatedAtUtc = startedAtUtc;
    }

    private ReadingAttempt()
    {
    }

    public Guid UserId { get; private init; }

    public ReadingPassageId ReadingPassageId { get; private init; }

    public Guid? StudySessionId { get; private init; }

    public Guid? WeeklyAssessmentId { get; private init; }

    public Guid? MockExamId { get; private init; }

    /// <summary>Server clock at attempt start.</summary>
    public DateTimeOffset StartedAtUtc { get; private init; }

    /// <summary>Server-computed deadline (<see cref="StartedAtUtc"/> + <see cref="ReadingPassage.DurationSeconds"/>). Never client-supplied.</summary>
    public DateTimeOffset DeadlineAtUtc { get; private init; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    /// <summary>JSON array of <c>{ questionId, submittedAnswer, isCorrect }</c> per question. Null until submitted.</summary>
    public string? AnswersJson { get; private set; }

    public int? RawScore { get; private set; }

    public int? MaxScore { get; private set; }

    public decimal? Band { get; private set; }

    public bool IsSubmitted => SubmittedAtUtc is not null;

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc > DeadlineAtUtc;

    public static ReadingAttempt Start(
        ReadingAttemptId id, Guid tenantId, Guid userId, ReadingPassageId passageId, DateTimeOffset nowUtc, int durationSeconds) =>
        new(id, tenantId, userId, passageId, nowUtc, nowUtc.AddSeconds(durationSeconds));

    /// <summary>
    /// Grades and closes the attempt. Idempotent by construction: <see cref="IsSubmitted"/> attempts
    /// refuse a second scoring pass (callers should treat an already-submitted attempt as success and
    /// return the stored result rather than call this again — ERD §4.3 concurrency strategy).
    /// </summary>
    public Result Submit(string answersJson, int rawScore, int maxScore, decimal band, DateTimeOffset nowUtc)
    {
        if (IsSubmitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_READING_ATTEMPT_ALREADY_SUBMITTED", "This attempt has already been submitted."));
        }

        AnswersJson = answersJson;
        RawScore = rawScore;
        MaxScore = maxScore;
        Band = band;
        SubmittedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new ReadingAttemptScoredDomainEvent(TenantId, UserId, ReadingPassageId.Value, Id.Value, rawScore, maxScore, band));
        return Result.Success();
    }
}
