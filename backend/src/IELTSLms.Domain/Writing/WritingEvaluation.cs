using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Writing;

/// <summary>
/// The AI-produced scoring result for a <see cref="WritingSubmission"/> (ERD ENT-029), 1:1 with its
/// submission (DDD §8.2 "one active evaluation per submission"). The four rubric criteria plus
/// <see cref="EstimatedBand"/>/<see cref="Confidence"/>/<see cref="FeedbackText"/> are AI-labeled and
/// write-once — there is no method that mutates them after creation (INV-05). A teacher override
/// (<see cref="Override"/>) only ever writes the separate <c>Overridden*</c> audit-trail fields,
/// leaving the original AI-produced values untouched and queryable forever.
/// </summary>
public sealed class WritingEvaluation : TenantAuditableEntity<WritingEvaluationId>
{
    private WritingEvaluation(
        WritingEvaluationId id, Guid tenantId, WritingSubmissionId writingSubmissionId, decimal taskResponse,
        decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal estimatedBand,
        decimal confidence, string feedbackText, decimal? similarityScore, bool requiresHumanReview, AiResponseId? aiResponseId,
        AiProviderId? aiProviderId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        WritingSubmissionId = writingSubmissionId;
        TaskResponse = taskResponse;
        CoherenceCohesion = coherenceCohesion;
        LexicalResource = lexicalResource;
        GrammaticalRangeAccuracy = grammaticalRangeAccuracy;
        EstimatedBand = estimatedBand;
        Confidence = confidence;
        FeedbackText = feedbackText;
        SimilarityScore = similarityScore;
        RequiresHumanReview = requiresHumanReview;
        AIResponseId = aiResponseId;
        AIProviderId = aiProviderId;
        CreatedAtUtc = nowUtc;
    }

    private WritingEvaluation()
    {
        FeedbackText = null!;
    }

    public WritingSubmissionId WritingSubmissionId { get; private init; }

    public decimal TaskResponse { get; private init; }

    public decimal CoherenceCohesion { get; private init; }

    public decimal LexicalResource { get; private init; }

    public decimal GrammaticalRangeAccuracy { get; private init; }

    public decimal EstimatedBand { get; private init; }

    /// <summary>Examiner-reported confidence, 0–1. Below <c>WritingEvaluationDefaults.LowConfidenceThreshold</c> sets <see cref="RequiresHumanReview"/>.</summary>
    public decimal Confidence { get; private init; }

    public string FeedbackText { get; private init; }

    /// <summary>
    /// The plagiarism/similarity-check tool's highest match score, 0-1, against the tenant's other
    /// submissions for the same task (Agentic AI Architecture §12.2's "plagiarism/similarity check"
    /// tool) — <see langword="null"/> when the check found no comparable prior submissions. Flags, never
    /// blocks: a high score only contributes to <see cref="RequiresHumanReview"/>, the submission is
    /// still scored and published.
    /// </summary>
    public decimal? SimilarityScore { get; private init; }

    /// <summary>Set when <see cref="Confidence"/> is low, or <see cref="SimilarityScore"/> is above threshold —
    /// surfaced for human review, never silently accepted (HITL, DDD §18 §12 footnote).</summary>
    public bool RequiresHumanReview { get; private init; }

    public AiResponseId? AIResponseId { get; private init; }

    public AiProviderId? AIProviderId { get; private init; }

    public decimal? OverriddenTaskResponse { get; private set; }

    public decimal? OverriddenCoherenceCohesion { get; private set; }

    public decimal? OverriddenLexicalResource { get; private set; }

    public decimal? OverriddenGrammaticalRangeAccuracy { get; private set; }

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

    public static Result<WritingEvaluation> Create(
        WritingEvaluationId id, Guid tenantId, WritingSubmissionId writingSubmissionId, decimal taskResponse,
        decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal confidence,
        string feedbackText, decimal lowConfidenceThreshold, decimal? similarityScore, decimal highSimilarityThreshold,
        AiResponseId? aiResponseId, AiProviderId? aiProviderId, DateTimeOffset nowUtc)
    {
        foreach (var score in new[] { taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy })
        {
            if (!IsValidCriterionScore(score))
            {
                return Result.Failure<WritingEvaluation>(WritingDomainErrors.RubricScoreOutOfRange);
            }
        }

        if (confidence is < 0m or > 1m)
        {
            return Result.Failure<WritingEvaluation>(WritingDomainErrors.ConfidenceOutOfRange);
        }

        if (similarityScore is < 0m or > 1m)
        {
            return Result.Failure<WritingEvaluation>(WritingDomainErrors.SimilarityScoreOutOfRange);
        }

        var estimatedBand = WritingBandCalculator.ToBand(taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy);

        // Escalation policy (Agentic AI Architecture §12.2): low confidence OR a similarity-check match
        // above threshold flags for human review — the similarity check flags, it never blocks scoring
        // or publication on its own.
        var requiresHumanReview = confidence < lowConfidenceThreshold || similarityScore >= highSimilarityThreshold;

        return Result.Success(new WritingEvaluation(
            id, tenantId, writingSubmissionId, taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy,
            estimatedBand, confidence, feedbackText, similarityScore, requiresHumanReview, aiResponseId, aiProviderId, nowUtc));
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
            return Result.Failure(WritingDomainErrors.ReviewDecisionRequired);
        }

        if (!RequiresHumanReview)
        {
            return Result.Failure(WritingDomainErrors.EvaluationNotFlaggedForReview);
        }

        ReviewDecision = decision;
        ReviewedBy = reviewedBy;
        ReviewedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    /// <summary>
    /// Records a teacher override. Writes only the <c>Overridden*</c> fields — the original
    /// AI-produced scores above are never mutated, preserving the audit trail (sprint requirement).
    /// </summary>
    public Result Override(
        decimal taskResponse, decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy,
        string reason, Guid overriddenBy, DateTimeOffset nowUtc)
    {
        foreach (var score in new[] { taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy })
        {
            if (!IsValidCriterionScore(score))
            {
                return Result.Failure(WritingDomainErrors.RubricScoreOutOfRange);
            }
        }

        OverriddenTaskResponse = taskResponse;
        OverriddenCoherenceCohesion = coherenceCohesion;
        OverriddenLexicalResource = lexicalResource;
        OverriddenGrammaticalRangeAccuracy = grammaticalRangeAccuracy;
        OverriddenBand = WritingBandCalculator.ToBand(taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy);
        OverrideReason = reason;
        OverriddenBy = overriddenBy;
        OverriddenAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    private static bool IsValidCriterionScore(decimal score) =>
        score is >= 0m and <= 9m && (score * 2m) == Math.Truncate(score * 2m);
}
