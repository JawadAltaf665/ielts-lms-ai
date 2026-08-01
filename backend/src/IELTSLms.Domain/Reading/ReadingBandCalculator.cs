namespace IELTSLms.Domain.Reading;

/// <summary>
/// Maps a raw score (0–40) to an IELTS band (0.0–9.0, 0.5 increments) using the standard published
/// Academic Reading conversion table (ERD §4.3: "Score 0–40 (raw) / band mapping valid").
/// </summary>
public static class ReadingBandCalculator
{
    public static decimal ToBand(int rawScore, int maxScore)
    {
        if (maxScore <= 0)
        {
            return 0m;
        }

        // Scale to a 40-item test if the passage's question count differs from the standard IELTS set.
        var scaled = (int)Math.Round(rawScore * 40m / maxScore, MidpointRounding.AwayFromZero);

        return scaled switch
        {
            >= 39 => 9.0m,
            >= 37 => 8.5m,
            >= 35 => 8.0m,
            >= 33 => 7.5m,
            >= 30 => 7.0m,
            >= 27 => 6.5m,
            >= 23 => 6.0m,
            >= 19 => 5.5m,
            >= 15 => 5.0m,
            >= 13 => 4.5m,
            >= 10 => 4.0m,
            >= 8 => 3.5m,
            >= 6 => 3.0m,
            >= 4 => 2.5m,
            _ => 0m,
        };
    }
}
