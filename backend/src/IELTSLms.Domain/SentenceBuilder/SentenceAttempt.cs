using IELTSLms.Domain.AI;
using IELTSLms.Domain.SentenceBuilder.Events;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.SentenceBuilder;

/// <summary>
/// A learner's constructed-sentence submission against a <see cref="SentenceExercise"/> (ERD ENT-020).
/// Grading is synchronous and deterministic (<see cref="SentencePatternMatcher"/>), so the attempt is
/// created already-scored and the scored fields never mutate afterward — same immutability-by-
/// construction as <see cref="Grammar.GrammarAttempt"/> (INV-04-style rule). <see cref="AiResponseId"/>/
/// <see cref="Feedback"/> are the one deliberate exception: they start <see langword="null"/> and are
/// populated exactly once by the async AI scaffolding job (additive enrichment, not a re-grade).
/// </summary>
public sealed class SentenceAttempt : TenantAuditableEntity<SentenceAttemptId>
{
    private SentenceAttempt(
        SentenceAttemptId id, Guid tenantId, Guid userId, SentenceExerciseId exerciseId, string submittedText,
        int score, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SentenceExerciseId = exerciseId;
        SubmittedText = submittedText;
        Score = score;
        CreatedAtUtc = nowUtc;
    }

    private SentenceAttempt()
    {
        SubmittedText = null!;
    }

    public Guid UserId { get; private init; }

    public SentenceExerciseId SentenceExerciseId { get; private init; }

    public string SubmittedText { get; private init; }

    /// <summary>Deterministic pattern-match percentage (0-100), always populated at submission.</summary>
    public int? Score { get; private init; }

    public AiResponseId? AiResponseId { get; private set; }

    public string? Feedback { get; private set; }

    public static SentenceAttempt Submit(
        SentenceAttemptId id, Guid tenantId, Guid userId, SentenceExerciseId exerciseId, string submittedText,
        int score, DateTimeOffset nowUtc)
    {
        var attempt = new SentenceAttempt(id, tenantId, userId, exerciseId, submittedText, score, nowUtc);
        attempt.RaiseDomainEvent(new SentenceAttemptSubmittedDomainEvent(
            tenantId, userId, exerciseId.Value, id.Value, submittedText));
        return attempt;
    }

    /// <summary>Attaches the AI scaffolding result once the async job completes. A no-op if already
    /// attached — the job's own idempotency guard should prevent a second call, this is defense in depth.</summary>
    public void AttachAiFeedback(AiResponseId aiResponseId, string feedback, DateTimeOffset nowUtc)
    {
        if (AiResponseId is not null)
        {
            return;
        }

        AiResponseId = aiResponseId;
        Feedback = feedback;
        UpdatedAtUtc = nowUtc;
    }
}
