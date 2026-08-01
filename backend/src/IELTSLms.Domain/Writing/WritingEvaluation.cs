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
        decimal confidence, string feedbackText, bool requiresHumanReview, AiResponseId? aiResponseId,
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

    /// <summary>Set when <see cref="Confidence"/> is low — surfaced for human review, never silently accepted (HITL, DDD §18 §12 footnote).</summary>
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

    public static Result<WritingEvaluation> Create(
        WritingEvaluationId id, Guid tenantId, WritingSubmissionId writingSubmissionId, decimal taskResponse,
        decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal confidence,
        string feedbackText, decimal lowConfidenceThreshold, AiResponseId? aiResponseId, AiProviderId? aiProviderId,
        DateTimeOffset nowUtc)
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

        var estimatedBand = WritingBandCalculator.ToBand(taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy);
        var requiresHumanReview = confidence < lowConfidenceThreshold;

        return Result.Success(new WritingEvaluation(
            id, tenantId, writingSubmissionId, taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy,
            estimatedBand, confidence, feedbackText, requiresHumanReview, aiResponseId, aiProviderId, nowUtc));
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
