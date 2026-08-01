using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Speaking;

/// <summary>
/// A learner's recorded response to a <see cref="SpeakingTask"/> (ERD ENT-031). Unlike
/// <c>WritingSubmission</c>, the audio is attached at <see cref="Create"/> (upload is synchronous, same
/// pattern as <c>ListeningAudio</c>), so this starts one state further along
/// (<see cref="SpeakingAttemptStatus.Recorded"/> rather than a draft-without-content state). Immutable
/// once submitted — there is deliberately no <c>Update</c>/<c>Edit</c> method; a correction is always a
/// new attempt, never a resave of this one.
/// </summary>
public sealed class SpeakingAttempt : TenantAuditableEntity<SpeakingAttemptId>
{
    private SpeakingAttempt(
        SpeakingAttemptId id, Guid tenantId, Guid userId, SpeakingTaskId speakingTaskId, FileAssetId fileId,
        int durationSeconds, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SpeakingTaskId = speakingTaskId;
        FileId = fileId;
        DurationSeconds = durationSeconds;
        Status = SpeakingAttemptStatus.Recorded;
        CreatedAtUtc = nowUtc;
    }

    private SpeakingAttempt()
    {
    }

    public Guid UserId { get; private init; }

    public SpeakingTaskId SpeakingTaskId { get; private init; }

    public FileAssetId FileId { get; private init; }

    public int DurationSeconds { get; private init; }

    /// <summary>Populated by the STT step once transcription succeeds; <see langword="null"/> until then.</summary>
    public string? TranscriptText { get; private set; }

    public SpeakingAttemptStatus Status { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public SpeakingEvaluationId? SpeakingEvaluationId { get; private set; }

    public static Result<SpeakingAttempt> Create(
        SpeakingAttemptId id, Guid tenantId, Guid userId, SpeakingTaskId speakingTaskId, FileAssetId fileId,
        int durationSeconds, DateTimeOffset nowUtc)
    {
        if (durationSeconds <= 0)
        {
            return Result.Failure<SpeakingAttempt>(SpeakingDomainErrors.DurationInvalid);
        }

        return Result.Success(new SpeakingAttempt(id, tenantId, userId, speakingTaskId, fileId, durationSeconds, nowUtc));
    }

    /// <summary>Transitions Recorded→Submitted. Rejects a second call on an already-submitted/evaluated/failed attempt.</summary>
    public Result Submit(DateTimeOffset nowUtc)
    {
        if (Status != SpeakingAttemptStatus.Recorded)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_SPEAKING_ATTEMPT_ALREADY_SUBMITTED", "This attempt has already been submitted."));
        }

        Status = SpeakingAttemptStatus.Submitted;
        SubmittedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new SpeakingSubmittedDomainEvent(TenantId, UserId, SpeakingTaskId.Value, Id.Value));
        return Result.Success();
    }

    /// <summary>Records the STT-produced transcript mid-workflow. Not a state transition — only valid while Submitted.</summary>
    public Result SetTranscript(string transcriptText, DateTimeOffset nowUtc)
    {
        if (Status != SpeakingAttemptStatus.Submitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_SPEAKING_ATTEMPT_NOT_PENDING_EVALUATION", "This attempt is not awaiting evaluation."));
        }

        TranscriptText = transcriptText;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    /// <summary>Attaches the completed AI evaluation and transitions Submitted→Evaluated.</summary>
    public Result AttachEvaluation(SpeakingEvaluationId evaluationId, decimal estimatedBand, DateTimeOffset nowUtc)
    {
        if (Status != SpeakingAttemptStatus.Submitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_SPEAKING_ATTEMPT_NOT_PENDING_EVALUATION", "This attempt is not awaiting evaluation."));
        }

        SpeakingEvaluationId = evaluationId;
        Status = SpeakingAttemptStatus.Evaluated;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new SpeakingEvaluatedDomainEvent(TenantId, UserId, SpeakingTaskId.Value, Id.Value, evaluationId.Value, estimatedBand));
        return Result.Success();
    }

    /// <summary>Marks the attempt Failed after an unrecoverable STT/AI-evaluation pipeline error.</summary>
    public Result MarkEvaluationFailed(DateTimeOffset nowUtc)
    {
        if (Status != SpeakingAttemptStatus.Submitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_SPEAKING_ATTEMPT_NOT_PENDING_EVALUATION", "This attempt is not awaiting evaluation."));
        }

        Status = SpeakingAttemptStatus.Failed;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}
