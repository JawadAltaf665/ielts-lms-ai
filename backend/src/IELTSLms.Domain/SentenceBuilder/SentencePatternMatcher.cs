using System.Text.Json;

namespace IELTSLms.Domain.SentenceBuilder;

/// <summary>
/// Deterministic, rule-based scoring of a learner's constructed sentence against a
/// <see cref="SentenceExercise.TargetPatternJson"/> (no AI call — Sprint 7's base-module gap-fill,
/// mirrors <see cref="Grammar.GrammarAnswerGrader"/>'s "highest-risk pure logic" role). The target
/// pattern is a JSON array of required tokens (words/phrases) the pattern is built from, e.g.
/// <c>["although", "would"]</c> for a concessive-clause pattern. A submission earns credit per token
/// found (case-insensitive substring match), searched left-to-right so tokens found in the pattern's
/// order consume no earlier text twice; a token missed does not block later tokens from still matching.
/// Score is a 0-100 percentage of tokens matched. Pure and framework-free so it is exhaustively
/// unit-testable without mocks.
/// </summary>
public static class SentencePatternMatcher
{
    public static int Score(string targetPatternJson, string submittedText)
    {
        var tokens = ParsePattern(targetPatternJson);
        if (tokens.Length == 0)
        {
            return 0;
        }

        var normalized = submittedText.ToUpperInvariant();
        var searchFrom = 0;
        var matched = 0;

        foreach (var token in tokens)
        {
            var index = normalized.IndexOf(token.ToUpperInvariant(), searchFrom, StringComparison.Ordinal);
            if (index < 0)
            {
                continue;
            }

            matched++;
            searchFrom = index + token.Length;
        }

        return matched * 100 / tokens.Length;
    }

    private static string[] ParsePattern(string targetPatternJson)
    {
        if (string.IsNullOrWhiteSpace(targetPatternJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(targetPatternJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
