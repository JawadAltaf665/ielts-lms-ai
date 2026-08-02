using IELTSLms.Domain.Speaking;

namespace IELTSLms.Domain.Tests.Speaking;

public sealed class SpeakingEvaluationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const decimal LowConfidenceThreshold = 0.7m;
    private const decimal MaxCriterionSpread = 3.0m;

    private static SpeakingEvaluation CreateEvaluation(
        decimal confidence = 0.9m, decimal fluencyCoherence = 7.0m, decimal lexicalResource = 6.5m,
        decimal grammaticalRangeAccuracy = 7.0m, decimal pronunciation = 6.5m) =>
        SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), SpeakingAttemptId.From(Guid.NewGuid()), fluencyCoherence,
            lexicalResource, grammaticalRangeAccuracy, pronunciation, confidence, "Good fluency, some pronunciation issues.",
            LowConfidenceThreshold, MaxCriterionSpread, aiResponseId: null, aiProviderId: null, Now)
            .Value;

    [Fact]
    public void Create_computes_the_estimated_band_from_the_four_criteria()
    {
        var evaluation = CreateEvaluation();

        Assert.Equal(7.0m, evaluation.EstimatedBand);
        Assert.False(evaluation.RequiresHumanReview);
        Assert.False(evaluation.IsOverridden);
    }

    [Fact]
    public void Create_flags_low_confidence_for_human_review_rather_than_silently_accepting_it()
    {
        var evaluation = CreateEvaluation(confidence: 0.5m);

        Assert.True(evaluation.RequiresHumanReview);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9.25)]
    [InlineData(9.5)]
    public void Create_with_a_criterion_score_outside_0_to_9_in_half_steps_fails(decimal invalidScore)
    {
        var result = SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), SpeakingAttemptId.From(Guid.NewGuid()), invalidScore, 6.5m, 7.0m,
            6.5m, 0.9m, "feedback", LowConfidenceThreshold, MaxCriterionSpread, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE", result.Error.Code);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Create_with_confidence_outside_0_to_1_fails(decimal invalidConfidence)
    {
        var result = SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), SpeakingAttemptId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            invalidConfidence, "feedback", LowConfidenceThreshold, MaxCriterionSpread, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_CONFIDENCE_OUT_OF_RANGE", result.Error.Code);
    }

    [Fact]
    public void Create_flags_an_inconsistent_criterion_spread_for_human_review_even_at_high_confidence()
    {
        var evaluation = CreateEvaluation(confidence: 0.95m, fluencyCoherence: 8.5m, pronunciation: 5.0m);

        Assert.True(evaluation.RequiresHumanReview);
    }

    [Fact]
    public void MarkReviewed_records_the_decision_and_clears_the_evaluation_from_the_review_queue_signal()
    {
        var evaluation = CreateEvaluation(confidence: 0.5m);
        var teacherId = Guid.NewGuid();

        var result = evaluation.MarkReviewed("Confirmed", teacherId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.True(evaluation.IsReviewed);
        Assert.Equal("Confirmed", evaluation.ReviewDecision);
        Assert.Equal(teacherId, evaluation.ReviewedBy);
        Assert.Equal(Now.AddDays(1), evaluation.ReviewedAtUtc);
    }

    [Fact]
    public void MarkReviewed_on_an_evaluation_not_flagged_for_review_fails()
    {
        var evaluation = CreateEvaluation();

        var result = evaluation.MarkReviewed("Confirmed", Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_EVALUATION_NOT_FLAGGED_FOR_REVIEW", result.Error.Code);
    }

    [Fact]
    public void MarkReviewed_with_an_empty_decision_fails()
    {
        var evaluation = CreateEvaluation(confidence: 0.5m);

        var result = evaluation.MarkReviewed(" ", Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_REVIEW_DECISION_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Override_records_the_overridden_fields_without_mutating_the_original_AI_scores()
    {
        var evaluation = CreateEvaluation();
        var teacherId = Guid.NewGuid();
        var originalFluencyCoherence = evaluation.FluencyCoherence;
        var originalBand = evaluation.EstimatedBand;

        var result = evaluation.Override(6.0m, 6.0m, 6.0m, 6.0m, "Too generous on Fluency.", teacherId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.True(evaluation.IsOverridden);
        Assert.Equal(6.0m, evaluation.OverriddenFluencyCoherence);
        Assert.Equal(6.0m, evaluation.OverriddenBand);
        Assert.Equal(teacherId, evaluation.OverriddenBy);
        Assert.Equal("Too generous on Fluency.", evaluation.OverrideReason);

        // The original AI-produced values are never mutated (audit trail requirement).
        Assert.Equal(originalFluencyCoherence, evaluation.FluencyCoherence);
        Assert.Equal(originalBand, evaluation.EstimatedBand);
    }

    [Fact]
    public void Override_with_an_out_of_range_score_fails()
    {
        var evaluation = CreateEvaluation();

        var result = evaluation.Override(9.25m, 6.0m, 6.0m, 6.0m, "reason", Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE", result.Error.Code);
        Assert.False(evaluation.IsOverridden);
    }
}
