using IELTSLms.Domain.Writing;

namespace IELTSLms.Domain.Tests.Writing;

public sealed class WritingBandCalculatorTests
{
    [Theory]
    [InlineData(7, 7, 7, 7, 7.0)]
    [InlineData(6, 6, 7, 7, 6.5)]
    [InlineData(5.5, 6, 6, 6.5, 6.0)]
    [InlineData(9, 9, 9, 9, 9.0)]
    [InlineData(0, 0, 0, 0, 0.0)]
    [InlineData(6, 6.5, 7, 7.5, 7.0)]
    public void ToBand_averages_the_four_criteria_and_rounds_to_the_nearest_half_band(
        decimal taskResponse, decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal expected)
    {
        var band = WritingBandCalculator.ToBand(taskResponse, coherenceCohesion, lexicalResource, grammaticalRangeAccuracy);

        Assert.Equal(expected, band);
    }
}
