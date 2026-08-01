using IELTSLms.Domain.Vocabulary.Events;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Vocabulary;

/// <summary>
/// Per-learner spaced-repetition state for one <see cref="VocabularyWord"/> (ERD ENT-018, extended per
/// Sprint-2-Vocabulary-Module.md with SM-2 fields). One row per (<see cref="UserId"/>,
/// <see cref="VocabularyWordId"/>); <see cref="Grade"/> is both the "attempt" and the progress update in
/// one step — grading is synchronous and deterministic (the SM-2 formula), so writes are idempotent per
/// call (each call raises <see cref="VocabularyCompletedDomainEvent"/> exactly once), and concurrent
/// grading of the same row is guarded by the entity's own optimistic-concurrency <c>Version</c> rather
/// than an application-level idempotency key.
/// </summary>
public sealed class VocabularyProgress : TenantAuditableEntity<VocabularyProgressId>
{
    private VocabularyProgress(VocabularyProgressId id, Guid tenantId, Guid userId, VocabularyWordId wordId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        VocabularyWordId = wordId;
        EaseFactor = 2.5m;
        IntervalDays = 0;
        RepetitionCount = 0;
        CreatedAtUtc = nowUtc;
    }

    private VocabularyProgress()
    {
    }

    public Guid UserId { get; private init; }

    public VocabularyWordId VocabularyWordId { get; private init; }

    public decimal EaseFactor { get; private set; }

    public int IntervalDays { get; private set; }

    public int RepetitionCount { get; private set; }

    /// <summary>When this word next comes due. <see langword="null"/> only before the first grade — a word
    /// with no progress row is already treated as due by the review-batch query, so this never needs a
    /// sentinel "due now" value.</summary>
    public DateTimeOffset? NextReviewAtUtc { get; private set; }

    /// <summary>The 0-5 SM-2 quality score from the most recent review.</summary>
    public int? LastGrade { get; private set; }

    public static VocabularyProgress Create(VocabularyProgressId id, Guid tenantId, Guid userId, VocabularyWordId wordId, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, wordId, nowUtc);

    /// <summary>Folds one review of <paramref name="quality"/> (0-5) into the SM-2 schedule and raises
    /// <see cref="VocabularyCompletedDomainEvent"/>.</summary>
    public void Grade(int quality, DateTimeOffset nowUtc)
    {
        var next = SpacedRepetitionScheduler.Schedule(EaseFactor, IntervalDays, RepetitionCount, quality);

        EaseFactor = next.EaseFactor;
        IntervalDays = next.IntervalDays;
        RepetitionCount = next.RepetitionCount;
        LastGrade = quality;
        NextReviewAtUtc = nowUtc.AddDays(next.IntervalDays);
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new VocabularyCompletedDomainEvent(TenantId, UserId, VocabularyWordId.Value, quality));
    }
}
