using IELTSLms.Domain.Writing.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Writing;

/// <summary>
/// A learner's response to a <see cref="WritingTask"/> (ERD ENT-028). Immutable once submitted
/// (INV-04) — there is deliberately no <c>Update</c>/<c>Edit</c> method; a correction is always a new
/// submission, never a resave of this one. Modeled with an explicit Draft→Submitted transition
/// (mirrors <c>ListeningAttempt.Submit</c>'s idempotency guard) even though this sprint's single POST
/// endpoint calls <see cref="Create"/> then <see cref="Submit"/> back-to-back — the guard is what
/// makes INV-04 an assertable, testable invariant rather than just "there's no endpoint for it."
/// </summary>
public sealed class WritingSubmission : TenantAuditableEntity<WritingSubmissionId>
{
    private WritingSubmission(
        WritingSubmissionId id, Guid tenantId, Guid userId, WritingTaskId writingTaskId, string responseText, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        WritingTaskId = writingTaskId;
        ResponseText = responseText;
        Status = WritingSubmissionStatus.Draft;
        CreatedAtUtc = nowUtc;
    }

    private WritingSubmission()
    {
        ResponseText = null!;
    }

    public Guid UserId { get; private init; }

    public WritingTaskId WritingTaskId { get; private init; }

    public string ResponseText { get; private init; }

    public int WordCount { get; private set; }

    public WritingSubmissionStatus Status { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public WritingEvaluationId? WritingEvaluationId { get; private set; }

    public static Result<WritingSubmission> Create(
        WritingSubmissionId id, Guid tenantId, Guid userId, WritingTaskId writingTaskId, string responseText, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return Result.Failure<WritingSubmission>(WritingDomainErrors.ResponseTextRequired);
        }

        return Result.Success(new WritingSubmission(id, tenantId, userId, writingTaskId, responseText, nowUtc));
    }

    /// <summary>
    /// Records the server-computed word count and transitions Draft→Submitted. Rejects a second call
    /// on an already-submitted/evaluated/failed submission (INV-04's assertable enforcement).
    /// </summary>
    public Result Submit(int wordCount, DateTimeOffset nowUtc)
    {
        if (Status != WritingSubmissionStatus.Draft)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_WRITING_SUBMISSION_ALREADY_SUBMITTED", "This submission has already been submitted."));
        }

        WordCount = wordCount;
        Status = WritingSubmissionStatus.Submitted;
        SubmittedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new WritingSubmittedDomainEvent(TenantId, UserId, WritingTaskId.Value, Id.Value));
        return Result.Success();
    }

    /// <summary>Attaches the completed AI evaluation and transitions Submitted→Evaluated (POL-03/POL-04 completion).</summary>
    public Result AttachEvaluation(WritingEvaluationId evaluationId, decimal estimatedBand, DateTimeOffset nowUtc)
    {
        if (Status != WritingSubmissionStatus.Submitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_WRITING_SUBMISSION_NOT_PENDING_EVALUATION", "This submission is not awaiting evaluation."));
        }

        WritingEvaluationId = evaluationId;
        Status = WritingSubmissionStatus.Evaluated;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new WritingEvaluatedDomainEvent(TenantId, UserId, WritingTaskId.Value, Id.Value, evaluationId.Value, estimatedBand));
        return Result.Success();
    }

    /// <summary>Marks the submission Failed after an unrecoverable AI-evaluation pipeline error.</summary>
    public Result MarkEvaluationFailed(DateTimeOffset nowUtc)
    {
        if (Status != WritingSubmissionStatus.Submitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_WRITING_SUBMISSION_NOT_PENDING_EVALUATION", "This submission is not awaiting evaluation."));
        }

        Status = WritingSubmissionStatus.Failed;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
