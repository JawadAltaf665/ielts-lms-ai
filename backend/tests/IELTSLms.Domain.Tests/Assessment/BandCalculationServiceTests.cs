using IELTSLms.Domain.Assessment;

namespace IELTSLms.Domain.Tests.Assessment;

public sealed class BandCalculationServiceTests
{
    // Official IELTS rounding examples (British Council/IDP): .25 rounds up to the next half band, .75
    // rounds up to the next whole band.
    [Theory]
    [InlineData(6.0, 6.0, 6.5, 6.5, 6.5)] // average 6.25 -> 6.5
    [InlineData(6.5, 6.5, 7.0, 7.0, 7.0)] // average 6.75 -> 7.0
    [InlineData(7.0, 7.0, 7.0, 7.0, 7.0)]
    [InlineData(0.0, 0.0, 0.0, 0.0, 0.0)]
    [InlineData(9.0, 9.0, 9.0, 9.0, 9.0)]
    [InlineData(6.5, 7.0, 6.5, 7.5, 7.0)] // average 6.875 -> nearer to 7.0
    [InlineData(6.0, 6.5, 6.0, 6.5, 6.5)] // average 6.25 -> 6.5 (regression on the tie rule with a different mix)
    public void CalculateOverallBand_matches_official_examples(
        decimal reading, decimal listening, decimal writing, decimal speaking, decimal expected)
    {
        var overall = BandCalculationService.CalculateOverallBand(reading, listening, writing, speaking);

        Assert.Equal(expected, overall);
    }

    [Fact]
    public void CalculateOverallBand_is_exhaustively_valid_across_the_full_0_to_9_half_band_input_space()
    {
        // Tier-1 combinatorial coverage (Sprint 8 testing notes): every section band is 0.0-9.0 in 0.5
        // steps (19 values), so this is 19^4 = 130,321 combinations - small and fast, but exhaustive.
        decimal[] bands = [0.0m, 0.5m, 1.0m, 1.5m, 2.0m, 2.5m, 3.0m, 3.5m, 4.0m, 4.5m, 5.0m, 5.5m, 6.0m, 6.5m, 7.0m, 7.5m, 8.0m, 8.5m, 9.0m];

        foreach (var reading in bands)
        {
            foreach (var listening in bands)
            {
                foreach (var writing in bands)
                {
                    foreach (var speaking in bands)
                    {
                        var overall = BandCalculationService.CalculateOverallBand(reading, listening, writing, speaking);

                        Assert.InRange(overall, 0.0m, 9.0m);
                        Assert.Equal(0m, (overall * 2m) % 1m); // always a multiple of 0.5

                        var average = (reading + listening + writing + speaking) / 4m;
                        Assert.InRange(Math.Abs(overall - average), 0m, 0.25m); // never rounds further than a quarter band away
                    }
                }
            }
        }
    }
}
