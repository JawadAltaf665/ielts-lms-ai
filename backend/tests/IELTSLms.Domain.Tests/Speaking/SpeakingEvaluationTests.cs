using IELTSLms.Domain.Speaking;

namespace IELTSLms.Domain.Tests.Speaking;

public sealed class SpeakingEvaluationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const decimal LowConfidenceThreshold = 0.7m;

    private static SpeakingEvaluation CreateEvaluation(decimal confidence = 0.9m) =>
        SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), SpeakingAttemptId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            confidence, "Good fluency, some pronunciation issues.", LowConfidenceThreshold, aiResponseId: null, aiProviderId: null, Now)
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
            6.5m, 0.9m, "feedback", LowConfidenceThreshold, null, null, Now);

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
            invalidConfidence, "feedback", LowConfidenceThreshold, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_CONFIDENCE_OUT_OF_RANGE", result.Error.Code);
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
