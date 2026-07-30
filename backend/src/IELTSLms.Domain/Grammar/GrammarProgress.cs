using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// Per-learner mastery of a <see cref="GrammarTopic"/> (mirrors ENT-018 VocabularyProgress exactly,
/// per the sprint's domain model table). One row per (<see cref="UserId"/>, <see cref="GrammarTopicId"/>);
/// updated from the <see cref="Events.GrammarCompletedDomainEvent"/> handler, so writes are idempotent
/// per attempt (each attempt fires the event exactly once).
/// </summary>
public sealed class GrammarProgress : TenantAuditableEntity<GrammarProgressId>
{
    private GrammarProgress(GrammarProgressId id, Guid tenantId, Guid userId, GrammarTopicId topicId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        GrammarTopicId = topicId;
        MasteryLevel = GrammarMasteryLevel.NotStarted;
        AttemptCount = 0;
        CreatedAtUtc = nowUtc;
    }

    private GrammarProgress()
    {
    }

    public Guid UserId { get; private init; }

    public GrammarTopicId GrammarTopicId { get; private init; }

    public GrammarMasteryLevel MasteryLevel { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset? LastAttemptedAtUtc { get; private set; }

    public static GrammarProgress Create(GrammarProgressId id, Guid tenantId, Guid userId, GrammarTopicId topicId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, topicId, nowUtc);

    /// <summary>
    /// Folds one scored attempt into this topic's mastery: mastered requires both a high accuracy on
    /// the latest attempt and enough repetitions (3+) to call it retained, not a lucky guess.
    /// </summary>
    public void RecordAttempt(int rawScore, int maxScore, DateTimeOffset nowUtc)
    {
        AttemptCount++;
        LastAttemptedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        var accuracy = maxScore > 0 ? (double)rawScore / maxScore : 0d;
        MasteryLevel = accuracy switch
        {
            >= 0.9 when AttemptCount >= 3 => GrammarMasteryLevel.Mastered,
            >= 0.7 => GrammarMasteryLevel.Practiced,
            _ => GrammarMasteryLevel.Learning,
        };
    }
}
