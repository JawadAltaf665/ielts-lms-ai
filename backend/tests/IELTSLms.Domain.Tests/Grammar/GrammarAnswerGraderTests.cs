using System.Text.Json;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Domain.Tests.Grammar;

/// <summary>
/// Exhaustive per-<see cref="GrammarQuestionType"/> coverage of the deterministic answer-key
/// matching (the sprint's highest-risk pure logic, per Sprint-1-Grammar-Module.md "Testing").
/// </summary>
public sealed class GrammarAnswerGraderTests
{
    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Theory]
    [InlineData("b", true)]
    [InlineData("B", true)]
    [InlineData(" b ", true)]
    [InlineData("c", false)]
    public void MultipleChoice_matches_case_insensitively_and_trims(string submitted, bool expected)
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(GrammarQuestionType.MultipleChoice, Key("b"), submitted);

        Assert.Equal(expected, isCorrect);
    }

    [Theory]
    [InlineData("cat", true)]
    [InlineData("cats", true)]
    [InlineData("CAT", true)]
    [InlineData("dog", false)]
    public void FillBlank_matches_any_acceptable_alternative(string submitted, bool expected)
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(GrammarQuestionType.FillBlank, Key("cat", "cats"), submitted);

        Assert.Equal(expected, isCorrect);
    }

    [Theory]
    [InlineData("The cat is sleeping.", true)]
    [InlineData("the cat is sleeping.", true)]
    [InlineData("The cat is sleep.", false)]
    public void ErrorCorrection_matches_the_corrected_sentence_exactly(string submitted, bool expected)
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(
            GrammarQuestionType.ErrorCorrection, Key("The cat is sleeping."), submitted);

        Assert.Equal(expected, isCorrect);
    }

    [Theory]
    [InlineData("3,1,4,2", true)]
    [InlineData(" 3, 1, 4, 2 ", true)]
    [InlineData("3,1,2,4", false)]
    [InlineData("3,1,4", false)]
    public void Reorder_requires_the_exact_token_order(string submitted, bool expected)
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(GrammarQuestionType.Reorder, Key("3,1,4,2"), submitted);

        Assert.Equal(expected, isCorrect);
    }

    [Fact]
    public void Null_submission_is_never_correct()
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(GrammarQuestionType.MultipleChoice, Key("b"), submittedAnswer: null);

        Assert.False(isCorrect);
    }

    [Fact]
    public void Malformed_answer_key_grades_as_incorrect_rather_than_throwing()
    {
        var isCorrect = GrammarAnswerGrader.IsCorrect(GrammarQuestionType.MultipleChoice, "not json", "b");

        Assert.False(isCorrect);
    }
}
