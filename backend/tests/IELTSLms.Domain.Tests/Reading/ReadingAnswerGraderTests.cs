using System.Text.Json;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Domain.Tests.Reading;

public sealed class ReadingAnswerGraderTests
{
    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Theory]
    [InlineData("B", "b", true)]
    [InlineData("B", "B", true)]
    [InlineData("B", " b ", true)]
    [InlineData("B", "c", false)]
    public void MultipleChoice_matches_trimmed_case_insensitive_option_letter(string key, string submitted, bool expected) =>
        Assert.Equal(expected, ReadingAnswerGrader.IsCorrect(ReadingQuestionType.MultipleChoice, Key(key), submitted));

    [Theory]
    [InlineData("TRUE", "true", true)]
    [InlineData("NOT GIVEN", "not given", true)]
    [InlineData("NOT GIVEN", "Not   Given", true)]
    [InlineData("NOT GIVEN", "NOTGIVEN", true)]
    [InlineData("TRUE", "false", false)]
    public void TrueFalseNotGiven_matches_whitespace_collapsed_case_insensitive(string key, string submitted, bool expected) =>
        Assert.Equal(expected, ReadingAnswerGrader.IsCorrect(ReadingQuestionType.TrueFalseNotGiven, Key(key), submitted));

    [Theory]
    [InlineData("YES", "yes", true)]
    [InlineData("NOT GIVEN", "not  given", true)]
    [InlineData("NO", "yes", false)]
    public void YesNoNotGiven_matches_whitespace_collapsed_case_insensitive(string key, string submitted, bool expected) =>
        Assert.Equal(expected, ReadingAnswerGrader.IsCorrect(ReadingQuestionType.YesNoNotGiven, Key(key), submitted));

    [Theory]
    [InlineData("iv", "IV", true)]
    [InlineData("iv", " iv ", true)]
    [InlineData("iv", "v", false)]
    public void MatchingHeadings_matches_trimmed_case_insensitive_heading_code(string key, string submitted, bool expected) =>
        Assert.Equal(expected, ReadingAnswerGrader.IsCorrect(ReadingQuestionType.MatchingHeadings, Key(key), submitted));

    [Theory]
    [InlineData("C", "c", true)]
    [InlineData("C", "d", false)]
    public void MatchingInformation_matches_trimmed_case_insensitive_paragraph_code(string key, string submitted, bool expected) =>
        Assert.Equal(expected, ReadingAnswerGrader.IsCorrect(ReadingQuestionType.MatchingInformation, Key(key), submitted));

    [Fact]
    public void SentenceCompletion_accepts_any_synonym_alternative_with_normalized_whitespace_and_case()
    {
        var key = Key("blue whale", "blue-whale");

        Assert.True(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.SentenceCompletion, key, "Blue   Whale"));
        Assert.False(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.SentenceCompletion, key, "grey whale"));
    }

    [Fact]
    public void SummaryCompletion_accepts_any_synonym_alternative_with_normalized_whitespace_and_case()
    {
        var key = Key("global warming");

        Assert.True(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.SummaryCompletion, key, "  GLOBAL   WARMING  "));
        Assert.False(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.SummaryCompletion, key, "climate change"));
    }

    [Fact]
    public void ShortAnswer_accepts_any_synonym_alternative()
    {
        var key = Key("dog", "puppy");

        Assert.True(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.ShortAnswer, key, "PUPPY"));
        Assert.False(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.ShortAnswer, key, "cat"));
    }

    [Fact]
    public void Null_submission_is_never_correct() =>
        Assert.False(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.ShortAnswer, Key("dog"), null));

    [Fact]
    public void Malformed_answer_key_json_is_treated_as_no_acceptable_answers() =>
        Assert.False(ReadingAnswerGrader.IsCorrect(ReadingQuestionType.ShortAnswer, "not-json", "dog"));
}
