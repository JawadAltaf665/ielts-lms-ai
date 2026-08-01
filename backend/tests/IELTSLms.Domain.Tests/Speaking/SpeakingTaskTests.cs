using IELTSLms.Domain.Speaking;

namespace IELTSLms.Domain.Tests.Speaking;

public sealed class SpeakingTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_data_succeeds_and_is_tenant_owned()
    {
        var tenantId = Guid.NewGuid();

        var result = SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), tenantId, "Part 2 - Describe a place", SpeakingPart.Part2,
            "Describe a place you like to visit.", categoryId: null, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Equal(SpeakingPart.Part2, result.Value.Part);
    }

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), tenantId: null, "Global Task", SpeakingPart.Part1, "Tell me about your hometown.",
            categoryId: null, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_empty_title_fails()
    {
        var result = SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), Guid.NewGuid(), "  ", SpeakingPart.Part1, "prompt", null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_TITLE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_empty_prompt_text_fails()
    {
        var result = SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", SpeakingPart.Part1, " ", null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_PROMPT_TEXT_REQUIRED", result.Error.Code);
    }
}
