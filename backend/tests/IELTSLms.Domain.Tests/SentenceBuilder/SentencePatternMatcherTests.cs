using System.Text.Json;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Domain.Tests.SentenceBuilder;

/// <summary>
/// Exhaustive coverage of the deterministic pattern-match scorer (Sprint 7's highest-risk pure logic,
/// mirrors <c>GrammarAnswerGraderTests</c>' role for Grammar's own scorer).
/// </summary>
public sealed class SentencePatternMatcherTests
{
    private static string Pattern(params string[] tokens) => JsonSerializer.Serialize(tokens);

    [Fact]
    public void All_tokens_present_in_order_scores_100()
    {
        var score = SentencePatternMatcher.Score(
            Pattern("although", "would"), "Although it was raining, she would still go for a run.");

        Assert.Equal(100, score);
    }

    [Fact]
    public void All_tokens_present_case_insensitively_scores_100()
    {
        var score = SentencePatternMatcher.Score(Pattern("although", "would"), "ALTHOUGH it rained, she WOULD go.");

        Assert.Equal(100, score);
    }

    [Fact]
    public void Missing_one_of_two_tokens_scores_50()
    {
        var score = SentencePatternMatcher.Score(Pattern("although", "would"), "Although it was raining, she went for a run.");

        Assert.Equal(50, score);
    }

    [Fact]
    public void Missing_all_tokens_scores_0()
    {
        var score = SentencePatternMatcher.Score(Pattern("although", "would"), "It was raining and she went for a run.");

        Assert.Equal(0, score);
    }

    [Fact]
    public void A_token_appearing_before_the_search_position_does_not_count_as_a_later_match()
    {
        // "would" appears in the text, but only *before* where "although" was found — since matching
        // is a forward-only scan in the pattern's declared order, "would" is not credited here.
        var score = SentencePatternMatcher.Score(Pattern("although", "would"), "She would go. Although it rained.");

        Assert.Equal(50, score);
    }

    [Fact]
    public void Tokens_matching_in_the_patterns_declared_order_both_score()
    {
        var score = SentencePatternMatcher.Score(Pattern("although", "would"), "Although it rained, she would go.");

        Assert.Equal(100, score);
    }

    [Fact]
    public void A_token_not_found_does_not_block_a_later_token_from_matching()
    {
        var score = SentencePatternMatcher.Score(Pattern("although", "would", "still"), "She would still go.");

        Assert.Equal(66, score);
    }

    [Fact]
    public void Empty_pattern_scores_0()
    {
        var score = SentencePatternMatcher.Score(Pattern(), "Any sentence at all.");

        Assert.Equal(0, score);
    }

    [Fact]
    public void Malformed_pattern_scores_0_rather_than_throwing()
    {
        var score = SentencePatternMatcher.Score("not json", "Any sentence at all.");

        Assert.Equal(0, score);
    }

    [Fact]
    public void Empty_submission_scores_0()
    {
        var score = SentencePatternMatcher.Score(Pattern("although"), "");

        Assert.Equal(0, score);
    }
}
