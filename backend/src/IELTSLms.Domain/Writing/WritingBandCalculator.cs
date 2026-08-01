namespace IELTSLms.Domain.Writing;

/// <summary>
/// Derives the estimated overall Writing band from the four AI-scored rubric criteria (DDD §8.3
/// <c>Evaluation</c> value object) — the simple average of Task Response, Coherence &amp; Cohesion,
/// Lexical Resource, and Grammatical Range &amp; Accuracy, rounded to the nearest 0.5 band. Unlike
/// <see cref="Reading.ReadingBandCalculator"/> (raw/max score ratio against a published conversion
/// table), Writing has no raw score — its four inputs are already bands, so this calculator only
/// averages and rounds.
/// </summary>
public static class WritingBandCalculator
{
    public static decimal ToBand(
        decimal taskResponse, decimal coherenceCohesion, decimal lexicalResource, decimal grammaticalRangeAccuracy)
    {
        var average = (taskResponse + coherenceCohesion + lexicalResource + grammaticalRangeAccuracy) / 4m;
        return Math.Round(average * 2m, MidpointRounding.AwayFromZero) / 2m;
    }
}
