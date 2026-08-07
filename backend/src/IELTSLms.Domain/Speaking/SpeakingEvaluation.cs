using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Speaking;

/// <summary>
/// The AI-produced scoring result for a <see cref="SpeakingAttempt"/> (ERD ENT-032), 1:1 with its
/// attempt. The four rubric criteria plus <see cref="EstimatedBand"/>/<see cref="Confidence"/>/
/// <see cref="FeedbackText"/> are AI-labeled and write-once — there is no method that mutates them
/// after creation. A teacher override (<see cref="Override"/>) only ever writes the separate
/// <c>Overridden*</c> audit-trail fields, leaving the original AI-produced values untouched and
/// queryable forever. Mirrors <see cref="Writing.WritingEvaluation"/> with Speaking's own criterion set.
/// </summary>
public sealed class SpeakingEvaluation : TenantAuditableEntity<SpeakingEvaluationId>
{
    private SpeakingEvaluation(
        SpeakingEvaluationId id, Guid tenantId, SpeakingAttemptId speakingAttemptId, decimal fluencyCoherence,
        decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal pronunciation, decimal estimatedBand,
        decimal confidence, string feedbackText, bool requiresHumanReview, AiResponseId? aiResponseId,
        AiProviderId? aiProviderId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        SpeakingAttemptId = speakingAttemptId;
        FluencyCoherence = fluencyCoherence;
        LexicalResource = lexicalResource;
        GrammaticalRangeAccuracy = grammaticalRangeAccuracy;
        Pronunciation = pronunciation;
        EstimatedBand = estimatedBand;
        Confidence = confidence;
        FeedbackText = feedbackText;
        RequiresHumanReview = requiresHumanReview;
        AIResponseId = aiResponseId;
        AIProviderId = aiProviderId;
        CreatedAtUtc = nowUtc;
    }

    private SpeakingEvaluation()
    {
        FeedbackText = null!;
    }

    public SpeakingAttemptId SpeakingAttemptId { get; private init; }

    public decimal FluencyCoherence { get; private init; }

    public decimal LexicalResource { get; private init; }

    public decimal GrammaticalRangeAccuracy { get; private init; }

    public decimal Pronunciation { get; private init; }

    public decimal EstimatedBand { get; private init; }

    /// <summary>Examiner-reported confidence, 0-1. Below the low-confidence threshold sets <see cref="RequiresHumanReview"/>.</summary>
    public decimal Confidence { get; private init; }

    public string FeedbackText { get; private init; }

    /// <summary>Set when <see cref="Confidence"/> is low — surfaced for human review, never silently accepted (HITL).</summary>
    public bool RequiresHumanReview { get; private init; }

    public AiResponseId? AIResponseId { get; private init; }

    public AiProviderId? AIProviderId { get; private init; }

    public decimal? OverriddenFluencyCoherence { get; private set; }

    public decimal? OverriddenLexicalResource { get; private set; }

    public decimal? OverriddenGrammaticalRangeAccuracy { get; private set; }

    public decimal? OverriddenPronunciation { get; private set; }

    public decimal? OverriddenBand { get; private set; }

    public Guid? OverriddenBy { get; private set; }

    public DateTimeOffset? OverriddenAtUtc { get; private set; }

    public string? OverrideReason { get; private set; }

    public bool IsOverridden => OverriddenAtUtc is not null;

    public Guid? ReviewedBy { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    /// <summary>The teacher's decision when resolving a flagged review (e.g. "Confirmed", "Escalated") — distinct from <see cref="Override"/>, which records a score change.</summary>
    public string? ReviewDecision { get; private set; }

    public bool IsReviewed => ReviewedAtUtc is not null;

    public static Result<SpeakingEvaluation> Create(
        SpeakingEvaluationId id, Guid tenantId, SpeakingAttemptId speakingAttemptId, decimal fluencyCoherence,
        decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal pronunciation, decimal confidence,
        string feedbackText, decimal lowConfidenceThreshold, decimal maxCriterionSpread, AiResponseId? aiResponseId,
        AiProviderId? aiProviderId, DateTimeOffset nowUtc)
    {
        var criteria = new[] { fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation };
        foreach (var score in criteria)
        {
            if (!IsValidCriterionScore(score))
            {
                return Result.Failure<SpeakingEvaluation>(SpeakingDomainErrors.RubricScoreOutOfRange);
            }
        }

        if (confidence is < 0m or > 1m)
        {
            return Result.Failure<SpeakingEvaluation>(SpeakingDomainErrors.ConfidenceOutOfRange);
        }

        var estimatedBand = SpeakingBandCalculator.ToBand(fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation);

        // Escalation policy (Agentic AI Architecture §10): low confidence OR internally-inconsistent
        // criteria (e.g. Pronunciation wildly divergent from Fluency without rationale) both flag for
        // human review — checked here in code after the model responds, never left to the model itself.
        var criterionSpread = criteria.Max() - criteria.Min();
        var requiresHumanReview = confidence < lowConfidenceThreshold || criterionSpread > maxCriterionSpread;

        return Result.Success(new SpeakingEvaluation(
            id, tenantId, speakingAttemptId, fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation,
            estimatedBand, confidence, feedbackText, requiresHumanReview, aiResponseId, aiProviderId, nowUtc));
    }

    /// <summary>
    /// Records a teacher's review decision on a flagged evaluation (audited: actor, timestamp, decision)
    /// — distinct from <see cref="Override"/>, which additionally changes the scores. A teacher may call
    /// either, or both (override first, then resolve, or vice versa); the review queue excludes an
    /// evaluation once it has been reviewed or overridden, whichever happens first.
    /// </summary>
    public Result MarkReviewed(string decision, Guid reviewedBy, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(decision))
        {
            return Result.Failure(SpeakingDomainErrors.ReviewDecisionRequired);
        }

        if (!RequiresHumanReview)
        {
            return Result.Failure(SpeakingDomainErrors.EvaluationNotFlaggedForReview);
        }

        ReviewDecision = decision;
        ReviewedBy = reviewedBy;
        ReviewedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    /// <summary>
    /// Records a teacher override. Writes only the <c>Overridden*</c> fields — the original AI-produced
    /// scores above are never mutated, preserving the audit trail.
    /// </summary>
    public Result Override(
        decimal fluencyCoherence, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal pronunciation,
        string reason, Guid overriddenBy, DateTimeOffset nowUtc)
    {
        foreach (var score in new[] { fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation })
        {
            if (!IsValidCriterionScore(score))
            {
                return Result.Failure(SpeakingDomainErrors.RubricScoreOutOfRange);
            }
        }

        OverriddenFluencyCoherence = fluencyCoherence;
        OverriddenLexicalResource = lexicalResource;
        OverriddenGrammaticalRangeAccuracy = grammaticalRangeAccuracy;
        OverriddenPronunciation = pronunciation;
        OverriddenBand = SpeakingBandCalculator.ToBand(fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation);
        OverrideReason = reason;
        OverriddenBy = overriddenBy;
        OverriddenAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    private static bool IsValidCriterionScore(decimal score) =>
        score is >= 0m and <= 9m && (score * 2m) == Math.Truncate(score * 2m);
}
