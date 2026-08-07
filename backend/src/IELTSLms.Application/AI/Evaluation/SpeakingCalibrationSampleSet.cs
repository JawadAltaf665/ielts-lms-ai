namespace IELTSLms.Application.AI.Evaluation;

/// <summary>
/// One held-out, expert-scored calibration item (Testing Strategy §8's "curated, versioned calibration
/// set"): a transcript plus the four criteria as an expert examiner scored them.
/// </summary>
public sealed record SpeakingCalibrationSample(
    string TaskPrompt, string Part, string Transcript, decimal ExpertFluencyCoherence, decimal ExpertLexicalResource,
    decimal ExpertGrammaticalRangeAccuracy, decimal ExpertPronunciation);

/// <summary>
/// ponytail: a small, developer-maintained in-code sample set rather than an admin-curated/queryable
/// store — enough to prove the calibration/gating mechanics end-to-end with a real statistical tolerance
/// check. Move to a managed, versioned store (with its own curation workflow) if the calibration set
/// needs to grow past what comfortably fits in a source file, or needs non-developer curation.
/// </summary>
public static class SpeakingCalibrationSampleSet
{
    public static IReadOnlyList<SpeakingCalibrationSample> Samples { get; } =
    [
        new(
            "Describe a place you like to visit in your free time.", "Part2",
            "Well, I'd like to talk about the small park near my apartment. I go there most weekends " +
            "because it's quiet and there's a nice pond with ducks. I usually just sit and read or " +
            "sometimes I meet a friend for coffee at the little cafe nearby. It's not a famous place at " +
            "all, but it means a lot to me because it reminds me of a park my grandmother used to take me " +
            "to when I was a child.",
            7.0m, 6.5m, 6.5m, 7.0m),
        new(
            "Do you prefer to work alone or in a team?", "Part1",
            "Um, I think, um, I prefer, um, team, because, um, more idea, and, um, I not good alone, um, " +
            "is difficult for me.",
            3.5m, 3.0m, 3.0m, 4.0m),
        new(
            "What changes would you like to see in your city in the future?", "Part3",
            "I would really like to see more investment in public transport, because right now the buses " +
            "are quite unreliable and a lot of people end up driving, which causes congestion and " +
            "pollution. If the city built a proper metro system, I think it would encourage people to " +
            "leave their cars at home. I'd also love to see more green spaces, since most of the parks " +
            "we have are quite small and crowded on weekends.",
            8.0m, 7.5m, 7.5m, 7.5m),
        new(
            "Tell me about a book you recently read.", "Part2",
            "Last month I read a book about, um, a detective, he is looking for a criminal in London. The " +
            "story was interesting but sometimes I don't understand all the words because the writer use " +
            "old English words. I like the ending because it was, um, surprise for me.",
            5.0m, 4.5m, 5.0m, 5.0m),
        new(
            "How important is it for people to learn a second language?", "Part3",
            "I'd say it's incredibly important, particularly in an increasingly globalized economy where " +
            "cross-border collaboration is the norm rather than the exception. Beyond the obvious career " +
            "advantages, there's compelling evidence that bilingualism enhances cognitive flexibility and " +
            "delays age-related cognitive decline, so the benefits extend well beyond the professional " +
            "sphere.",
            8.5m, 8.5m, 8.0m, 8.0m),
    ];
}
