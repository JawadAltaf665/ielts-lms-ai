namespace IELTSLms.Domain.Assessment;

/// <summary>
/// Aggregates the four section bands into the overall estimated band (DDD §8.2/§10.1 "Aggregate
/// section results into overall estimated band"). Pure domain service — no I/O.
///
/// Rule (IELTS official Overall Band Score guidance, British Council/IDP "How IELTS is scored"): the
/// four section bands are averaged, then rounded to the nearest half band; an average that lands
/// exactly on a quarter-band boundary always rounds UP (e.g. 6.25 -> 6.5, 6.75 -> 7.0). Since each
/// section band is itself a multiple of 0.5, the average of four such values is always a multiple of
/// 0.125, so this is implemented as the general rule "round to the nearest 0.5, ties round up" — which
/// reduces to exactly the official .25/.75 examples at the tie points, and resolves the remaining
/// eighth-band cases (.125/.375/.625/.875) to whichever half band they are numerically closer to.
/// </summary>
public static class BandCalculationService
{
    public static decimal CalculateOverallBand(decimal readingBand, decimal listeningBand, decimal writingBand, decimal speakingBand)
    {
        var average = (readingBand + listeningBand + writingBand + speakingBand) / 4m;
        return Math.Round(average * 2m, MidpointRounding.AwayFromZero) / 2m;
    }
}
