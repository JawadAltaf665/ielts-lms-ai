using System.Text.Json;

namespace IELTSLms.Domain.Reading;

/// <summary>
/// Deterministic, rule-based answer-key matching per <see cref="ReadingQuestionType"/> (Sprint 4:
/// "implement scoring as a strategy per Type, not one generic comparator, since ... normalization
/// rules differ"). The answer key is a JSON array of acceptable alternatives — a submission is correct
/// if it matches any alternative after the type's normalization. Pure and framework-free so it is
/// exhaustively unit-testable without mocks. Mirrors <c>GrammarAnswerGrader</c> (Phase 6 Sprint 1).
/// </summary>
public static class ReadingAnswerGrader
{
    public static bool IsCorrect(ReadingQuestionType type, string answerKeyJson, string? submittedAnswer)
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
    /// Three normalization groups per the sprint's scoring rules:
    /// - Exact-match short codes (option/heading/paragraph letters): trimmed, case-insensitive.
    /// - Fixed-vocabulary set-match (True/False/Not Given, Yes/No/Not Given): whitespace-collapsed,
    ///   case-insensitive, so "not given", "Not  Given", "NOTGIVEN" all normalize identically.
    /// - Free-text completion/short-answer: trimmed, internal whitespace collapsed to a single space,
    ///   case-insensitive — accepts any alternative in the key as a synonym.
    /// </summary>
    private static string Normalize(ReadingQuestionType type, string value) => type switch
    {
        ReadingQuestionType.TrueFalseNotGiven or ReadingQuestionType.YesNoNotGiven =>
            CollapseWhitespace(value).Replace(" ", string.Empty).ToUpperInvariant(),
        ReadingQuestionType.MultipleChoice or ReadingQuestionType.MatchingHeadings or ReadingQuestionType.MatchingInformation =>
            value.Trim().ToUpperInvariant(),
        _ => CollapseWhitespace(value).ToUpperInvariant(),
    };

    private static string CollapseWhitespace(string value) =>
        string.Join(' ', value.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
