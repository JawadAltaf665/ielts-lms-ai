using IELTSLms.Domain.Speaking;

namespace IELTSLms.Domain.Tests.Speaking;

public sealed class SpeakingBandCalculatorTests
{
    [Theory]
    [InlineData(7, 7, 7, 7, 7.0)]
    [InlineData(6, 6, 7, 7, 6.5)]
    [InlineData(5.5, 6, 6, 6.5, 6.0)]
    [InlineData(9, 9, 9, 9, 9.0)]
    [InlineData(0, 0, 0, 0, 0.0)]
    [InlineData(6, 6.5, 7, 7.5, 7.0)]
    public void ToBand_averages_the_four_criteria_and_rounds_to_the_nearest_half_band(
        decimal fluencyCoherence, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal pronunciation, decimal expected)
    {
        var band = SpeakingBandCalculator.ToBand(fluencyCoherence, lexicalResource, grammaticalRangeAccuracy, pronunciation);

        Assert.Equal(expected, band);
    }
}
