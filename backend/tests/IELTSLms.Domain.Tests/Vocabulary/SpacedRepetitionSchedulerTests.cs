using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Domain.Tests.Vocabulary;

/// <summary>
/// Exhaustive coverage of the simplified SM-2 formula (the sprint's highest-risk pure logic, per
/// Sprint-2-Vocabulary-Module.md "Testing": quality 0, 2, 3, 5; repetition 1/2/3+; ease-factor floor at 1.3).
/// </summary>
public sealed class SpacedRepetitionSchedulerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Quality_below_3_resets_repetition_and_interval_to_one(int quality)
    {
        var result = SpacedRepetitionScheduler.Schedule(easeFactor: 2.5m, intervalDays: 30, repetitionCount: 4, quality);

        Assert.Equal(0, result.RepetitionCount);
        Assert.Equal(1, result.IntervalDays);
    }

    [Fact]
    public void First_successful_repetition_sets_interval_to_one_day()
    {
        var result = SpacedRepetitionScheduler.Schedule(easeFactor: 2.5m, intervalDays: 0, repetitionCount: 0, quality: 4);

        Assert.Equal(1, result.RepetitionCount);
        Assert.Equal(1, result.IntervalDays);
    }

    [Fact]
    public void Second_successful_repetition_sets_interval_to_six_days()
    {
        var result = SpacedRepetitionScheduler.Schedule(easeFactor: 2.5m, intervalDays: 1, repetitionCount: 1, quality: 4);

        Assert.Equal(2, result.RepetitionCount);
        Assert.Equal(6, result.IntervalDays);
    }

    [Fact]
    public void Third_and_later_repetitions_multiply_previous_interval_by_the_incoming_ease_factor()
    {
        // Interval uses the ease factor as it stood *before* this review's adjustment (canonical SM-2 order).
        var result = SpacedRepetitionScheduler.Schedule(easeFactor: 2.5m, intervalDays: 6, repetitionCount: 2, quality: 5);

        Assert.Equal(3, result.RepetitionCount);
        Assert.Equal(15, result.IntervalDays); // round(6 * 2.5)
    }

    [Theory]
    [InlineData(5, 2.5, 2.6)]
    [InlineData(4, 2.5, 2.5)]
    [InlineData(3, 2.5, 2.36)]
    public void Ease_factor_adjusts_by_the_SM2_formula(int quality, double startingEaseFactor, double expectedEaseFactor)
    {
        var result = SpacedRepetitionScheduler.Schedule((decimal)startingEaseFactor, intervalDays: 6, repetitionCount: 2, quality);

        Assert.Equal((decimal)expectedEaseFactor, result.EaseFactor);
    }

    [Fact]
    public void Ease_factor_never_drops_below_the_floor_even_after_repeated_failures()
    {
        var easeFactor = 1.3m;

        for (var i = 0; i < 5; i++)
        {
            var result = SpacedRepetitionScheduler.Schedule(easeFactor, intervalDays: 1, repetitionCount: 0, quality: 0);
            easeFactor = result.EaseFactor;
        }

        Assert.Equal(SpacedRepetitionScheduler.MinimumEaseFactor, easeFactor);
    }

    [Fact]
    public void Ease_factor_still_adjusts_on_a_failed_review_not_just_repetition_and_interval()
    {
        var result = SpacedRepetitionScheduler.Schedule(easeFactor: 2.5m, intervalDays: 10, repetitionCount: 3, quality: 0);

        Assert.True(result.EaseFactor < 2.5m);
    }
}
