using IELTSLms.Domain.Writing;

namespace IELTSLms.Domain.Tests.Writing;

public sealed class WritingEvaluationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const decimal LowConfidenceThreshold = 0.7m;
    private const decimal HighSimilarityThreshold = 0.6m;

    private static WritingEvaluation CreateEvaluation(decimal confidence = 0.9m, decimal? similarityScore = null) =>
        WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            confidence, "Good structure, some grammar errors.", LowConfidenceThreshold, similarityScore, HighSimilarityThreshold,
            aiResponseId: null, aiProviderId: null, Now).Value;

    [Fact]
    public void Create_computes_the_estimated_band_from_the_four_criteria()
    {
        var evaluation = CreateEvaluation();

        Assert.Equal(7.0m, evaluation.EstimatedBand);
        Assert.False(evaluation.RequiresHumanReview);
        Assert.False(evaluation.IsOverridden);
        Assert.Null(evaluation.SimilarityScore);
    }

    [Fact]
    public void Create_flags_low_confidence_for_human_review_rather_than_silently_accepting_it()
    {
        var evaluation = CreateEvaluation(confidence: 0.5m);

        Assert.True(evaluation.RequiresHumanReview);
    }

    [Fact]
    public void Create_flags_a_high_similarity_score_for_human_review_even_at_high_confidence()
    {
        var evaluation = CreateEvaluation(confidence: 0.95m, similarityScore: 0.75m);

        Assert.True(evaluation.RequiresHumanReview);
        Assert.Equal(0.75m, evaluation.SimilarityScore);
    }

    [Fact]
    public void Create_with_a_similarity_score_below_threshold_does_not_flag_on_its_own()
    {
        var evaluation = CreateEvaluation(confidence: 0.95m, similarityScore: 0.2m);

        Assert.False(evaluation.RequiresHumanReview);
        Assert.Equal(0.2m, evaluation.SimilarityScore);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Create_with_a_similarity_score_outside_0_to_1_fails(decimal invalidScore)
    {
        var result = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            0.9m, "feedback", LowConfidenceThreshold, invalidScore, HighSimilarityThreshold, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_SIMILARITY_SCORE_OUT_OF_RANGE", result.Error.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9.25)]
    [InlineData(9.5)]
    public void Create_with_a_criterion_score_outside_0_to_9_in_half_steps_fails(decimal invalidScore)
    {
        var result = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), WritingSubmissionId.From(Guid.NewGuid()), invalidScore, 6.5m, 7.0m, 6.5m,
            0.9m, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE", result.Error.Code);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Create_with_confidence_outside_0_to_1_fails(decimal invalidConfidence)
    {
        var result = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), Guid.NewGuid(), WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m,
            invalidConfidence, "feedback", LowConfidenceThreshold, null, HighSimilarityThreshold, null, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_CONFIDENCE_OUT_OF_RANGE", result.Error.Code);
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
        Assert.Equal("CONFLICT_WRITING_EVALUATION_NOT_FLAGGED_FOR_REVIEW", result.Error.Code);
    }

    [Fact]
    public void MarkReviewed_with_an_empty_decision_fails()
    {
        var evaluation = CreateEvaluation(confidence: 0.5m);

        var result = evaluation.MarkReviewed(" ", Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_REVIEW_DECISION_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Override_records_the_overridden_fields_without_mutating_the_original_AI_scores()
    {
        var evaluation = CreateEvaluation();
        var teacherId = Guid.NewGuid();
        var originalTaskResponse = evaluation.TaskResponse;
        var originalBand = evaluation.EstimatedBand;

        var result = evaluation.Override(6.0m, 6.0m, 6.0m, 6.0m, "Too generous on Task Response.", teacherId, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.True(evaluation.IsOverridden);
        Assert.Equal(6.0m, evaluation.OverriddenTaskResponse);
        Assert.Equal(6.0m, evaluation.OverriddenBand);
        Assert.Equal(teacherId, evaluation.OverriddenBy);
        Assert.Equal("Too generous on Task Response.", evaluation.OverrideReason);

        // The original AI-produced values are never mutated (audit trail requirement).
        Assert.Equal(originalTaskResponse, evaluation.TaskResponse);
        Assert.Equal(originalBand, evaluation.EstimatedBand);
    }

    [Fact]
    public void Override_with_an_out_of_range_score_fails()
    {
        var evaluation = CreateEvaluation();

        var result = evaluation.Override(9.25m, 6.0m, 6.0m, 6.0m, "reason", Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE", result.Error.Code);
        Assert.False(evaluation.IsOverridden);
    }
}
