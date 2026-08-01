using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class BandPredictionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static BandPrediction CreatePending(out Guid tenantId, out Guid userId)
    {
        tenantId = Guid.NewGuid();
        userId = Guid.NewGuid();
        return BandPrediction.Enqueue(
            BandPredictionId.From(Guid.NewGuid()), tenantId, userId, AiOperationId.From(Guid.NewGuid()), Now);
    }

    [Fact]
    public void Enqueue_starts_pending()
    {
        var prediction = CreatePending(out _, out _);

        Assert.Equal(BandPredictionStatus.Pending, prediction.Status);
        Assert.Null(prediction.OverallBand);
    }

    [Fact]
    public void Complete_with_high_confidence_does_not_recommend_more_practice()
    {
        var prediction = CreatePending(out _, out _);

        var result = prediction.Complete(6.5m, 6.0m, 7.0m, 6.5m, 6.5m, 0.9m, AiResponseId.From(Guid.NewGuid()), Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(BandPredictionStatus.Ready, prediction.Status);
        Assert.Equal(6.5m, prediction.OverallBand);
        Assert.False(prediction.RecommendMorePractice);
    }

    [Fact]
    public void Complete_below_the_low_confidence_threshold_recommends_more_practice_instead_of_a_firm_number()
    {
        var prediction = CreatePending(out _, out _);

        var result = prediction.Complete(6.5m, null, null, 6.5m, null, 0.4m, AiResponseId.From(Guid.NewGuid()), Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.True(prediction.RecommendMorePractice);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Complete_with_confidence_outside_0_to_1_fails(decimal invalidConfidence)
    {
        var prediction = CreatePending(out _, out _);

        var result = prediction.Complete(6.5m, null, null, null, null, invalidConfidence, AiResponseId.From(Guid.NewGuid()), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_BAND_PREDICTION_CONFIDENCE_OUT_OF_RANGE", result.Error.Code);
        Assert.Equal(BandPredictionStatus.Pending, prediction.Status);
    }

    [Fact]
    public void Complete_with_a_band_outside_0_to_9_in_half_steps_fails()
    {
        var prediction = CreatePending(out _, out _);

        var result = prediction.Complete(9.25m, null, null, null, null, 0.9m, AiResponseId.From(Guid.NewGuid()), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_BAND_PREDICTION_BAND_OUT_OF_RANGE", result.Error.Code);
    }

    [Fact]
    public void Fail_transitions_to_failed_and_records_the_error()
    {
        var prediction = CreatePending(out _, out _);

        prediction.Fail("SERVER_UNEXPECTED", "provider unavailable", Now.AddMinutes(1));

        Assert.Equal(BandPredictionStatus.Failed, prediction.Status);
        Assert.Equal("SERVER_UNEXPECTED", prediction.ErrorCode);
    }

    [Fact]
    public void Complete_on_an_already_terminal_prediction_is_a_no_op()
    {
        var prediction = CreatePending(out _, out _);
        prediction.Fail("SERVER_UNEXPECTED", "boom", Now.AddMinutes(1));

        var result = prediction.Complete(6.5m, null, null, null, null, 0.9m, AiResponseId.From(Guid.NewGuid()), Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(BandPredictionStatus.Failed, prediction.Status);
        Assert.Null(prediction.OverallBand);
    }
}
