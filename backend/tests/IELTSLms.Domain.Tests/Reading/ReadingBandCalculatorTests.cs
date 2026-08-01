using IELTSLms.Domain.Reading;

namespace IELTSLms.Domain.Tests.Reading;

public sealed class ReadingBandCalculatorTests
{
    [Theory]
    [InlineData(40, 9.0)]
    [InlineData(39, 9.0)]
    [InlineData(37, 8.5)]
    [InlineData(30, 7.0)]
    [InlineData(23, 6.0)]
    [InlineData(0, 0.0)]
    public void Maps_a_40_item_raw_score_to_the_standard_academic_band(int rawScore, decimal expectedBand) =>
        Assert.Equal(expectedBand, ReadingBandCalculator.ToBand(rawScore, maxScore: 40));

    [Fact]
    public void Scales_a_non_standard_question_count_to_the_40_item_scale()
    {
        // 8/10 correct scales to 32/40, which sits in the 7.0 tier.
        Assert.Equal(7.0m, ReadingBandCalculator.ToBand(rawScore: 8, maxScore: 10));
    }

    [Fact]
    public void Zero_questions_yields_a_zero_band_rather_than_dividing_by_zero() =>
        Assert.Equal(0m, ReadingBandCalculator.ToBand(rawScore: 0, maxScore: 0));

    [Fact]
    public void Every_band_is_within_the_valid_0_to_9_half_step_range()
    {
        for (var raw = 0; raw <= 40; raw++)
        {
            var band = ReadingBandCalculator.ToBand(raw, 40);
            Assert.InRange(band, 0.0m, 9.0m);
            Assert.Equal(0m, (band * 2) % 1); // half-band increments only
        }
    }
}
