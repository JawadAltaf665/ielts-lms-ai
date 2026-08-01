namespace IELTSLms.Domain.Speaking;

/// <summary>
/// Derives the estimated overall Speaking band from the four AI-scored rubric criteria — the simple
/// average of Fluency &amp; Coherence, Lexical Resource, Grammatical Range &amp; Accuracy, and
/// Pronunciation, rounded to the nearest 0.5 band. Same shape as <see cref="Writing.WritingBandCalculator"/>,
/// kept as its own class per the codebase's one-calculator-per-module convention.
/// </summary>
public static class SpeakingBandCalculator
{
    public static decimal ToBand(
        decimal fluencyCoherence, decimal lexicalResource, decimal grammaticalRangeAccuracy, decimal pronunciation)
    {
        var average = (fluencyCoherence + lexicalResource + grammaticalRangeAccuracy + pronunciation) / 4m;
        return Math.Round(average * 2m, MidpointRounding.AwayFromZero) / 2m;
    }
}
