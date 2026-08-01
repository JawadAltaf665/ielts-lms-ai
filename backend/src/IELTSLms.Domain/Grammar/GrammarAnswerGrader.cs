using System.Text.Json;

namespace IELTSLms.Domain.Grammar;

/// <summary>
/// Deterministic, rule-based answer-key matching (no AI call — DDD §13.7 out-of-scope note). The
/// answer key is a JSON array of acceptable alternatives (<c>["b"]</c>, <c>["the cat is sleeping"]</c>);
/// a submission is correct if it matches any alternative after type-appropriate normalization.
/// Pure and framework-free so it is exhaustively unit-testable per <see cref="GrammarQuestionType"/>
/// without mocks (the sprint's highest-risk pure logic).
/// </summary>
public static class GrammarAnswerGrader
{
    public static bool IsCorrect(GrammarQuestionType type, string answerKeyJson, string? submittedAnswer)
    {
        if (submittedAnswer is null)
        {
            return false;
        }

        var normalizedSubmitted = Normalize(type, submittedAnswer);
        return ParseAnswerKey(answerKeyJson).Any(alternative => Normalize(type, alternative) == normalizedSubmitted);
    }

    private static string[] ParseAnswerKey(string answerKeyJson)
    {
        if (string.IsNullOrWhiteSpace(answerKeyJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(answerKeyJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Multiple-choice/fill-blank/error-correction match on trimmed, case-insensitive text.
    /// Reorder matches on the exact comma-separated token sequence (order matters).
    /// </summary>
    private static string Normalize(GrammarQuestionType type, string value) => type switch
    {
        GrammarQuestionType.Reorder => string.Join(',', SplitTokens(value)),
        _ => value.Trim().ToUpperInvariant(),
    };

    private static IEnumerable<string> SplitTokens(string value) =>
        value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.ToUpperInvariant());
}
