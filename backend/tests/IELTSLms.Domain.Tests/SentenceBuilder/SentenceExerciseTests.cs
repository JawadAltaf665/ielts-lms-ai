using System.Text.Json;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Domain.Tests.SentenceBuilder;

public sealed class SentenceExerciseTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Pattern(params string[] tokens) => JsonSerializer.Serialize(tokens);

    [Fact]
    public void Create_with_valid_prompt_and_pattern_succeeds()
    {
        var result = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), Guid.NewGuid(), "Write a concessive sentence.",
            Pattern("although", "would"), SentenceDifficulty.Intermediate, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), tenantId: null, "Write a concessive sentence.",
            Pattern("although"), SentenceDifficulty.Beginner, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_prompt(string prompt)
    {
        var result = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), Guid.NewGuid(), prompt, Pattern("although"), SentenceDifficulty.Beginner, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SENTENCE_EXERCISE_PROMPT_REQUIRED", result.Error.Code);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("not json")]
    [InlineData("")]
    public void Create_rejects_a_target_pattern_with_no_tokens(string targetPatternJson)
    {
        var result = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), Guid.NewGuid(), "A prompt", targetPatternJson, SentenceDifficulty.Beginner, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SENTENCE_EXERCISE_TARGET_PATTERN_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void SoftDelete_is_idempotent()
    {
        var exercise = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), Guid.NewGuid(), "A prompt", Pattern("although"), SentenceDifficulty.Beginner, Now).Value;

        exercise.SoftDelete(Now);
        exercise.SoftDelete(Now.AddDays(1));

        Assert.True(exercise.IsDeleted);
        Assert.Equal(Now, exercise.DeletedAtUtc);
    }
}
