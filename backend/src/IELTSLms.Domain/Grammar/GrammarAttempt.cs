using IELTSLms.Domain.Grammar.Events;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// A learner's scored submission against a <see cref="GrammarExercise"/> (ERD ENT-016, mirrors
/// ENT-023 ReadingAttempts). Grading is synchronous and deterministic (no AI, no draft state), so the
/// attempt is created already-scored and never mutated afterward — immutability by construction is
/// what makes re-submission safe: resubmitting simply creates another independent attempt, it can
/// never re-score or overwrite an existing one (INV-04-style rule, business rules §6).
/// <see cref="AnswersJson"/> holds the per-question results (question id, submitted answer,
/// correctness) as a JSON array — same <c>jsonb</c>-string convention as
/// <c>FailedWebhookEvent.RawJson</c>; there is no separate child-answers table.
/// </summary>
public sealed class GrammarAttempt : TenantAuditableEntity<GrammarAttemptId>
{
    private GrammarAttempt(
        GrammarAttemptId id, Guid tenantId, Guid userId, GrammarExerciseId exerciseId, string answersJson,
        int rawScore, int maxScore, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        GrammarExerciseId = exerciseId;
        AnswersJson = answersJson;
        RawScore = rawScore;
        MaxScore = maxScore;
        CreatedAtUtc = nowUtc;
    }

    private GrammarAttempt()
    {
        AnswersJson = null!;
    }

    public Guid UserId { get; private init; }

    public GrammarExerciseId GrammarExerciseId { get; private init; }

    /// <summary>JSON array of <c>{ questionId, submittedAnswer, isCorrect }</c> per question.</summary>
    public string AnswersJson { get; private init; }

    public int RawScore { get; private init; }

    public int MaxScore { get; private init; }

    public static GrammarAttempt Submit(
        GrammarAttemptId id, Guid tenantId, Guid userId, GrammarExerciseId exerciseId, Guid topicId,
        string answersJson, int rawScore, int maxScore, DateTimeOffset nowUtc)
    {
        var attempt = new GrammarAttempt(id, tenantId, userId, exerciseId, answersJson, rawScore, maxScore, nowUtc);
        attempt.RaiseDomainEvent(new GrammarCompletedDomainEvent(
            tenantId, userId, topicId, exerciseId.Value, id.Value, rawScore, maxScore));
        return attempt;
    }
}
