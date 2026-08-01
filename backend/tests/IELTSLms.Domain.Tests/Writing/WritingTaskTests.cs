using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Domain.Tests.Writing;

public sealed class WritingTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_data_succeeds_and_is_tenant_owned()
    {
        var tenantId = Guid.NewGuid();

        var result = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId, "Task 2 - Opinion Essay", WritingTaskType.Task2, ReadingTrack.Academic,
            "Some people think... Discuss.", "band-descriptors-v1", 250, 400, categoryId: null, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Equal(250, result.Value.MinWordCount);
        Assert.Equal(400, result.Value.MaxWordCount);
    }

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId: null, "Global Task", WritingTaskType.Task1, ReadingTrack.General,
            "Write a letter...", null, 150, 200, categoryId: null, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_empty_title_fails()
    {
        var result = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), Guid.NewGuid(), "  ", WritingTaskType.Task1, ReadingTrack.Academic, "prompt", null, 150, 200,
            null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_TITLE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_empty_prompt_text_fails()
    {
        var result = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", WritingTaskType.Task1, ReadingTrack.Academic, " ", null, 150, 200,
            null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_PROMPT_TEXT_REQUIRED", result.Error.Code);
    }

    [Theory]
    [InlineData(0, 200)]
    [InlineData(150, 0)]
    [InlineData(300, 200)]
    public void Create_with_invalid_word_count_bounds_fails(int minWordCount, int maxWordCount)
    {
        var result = WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", WritingTaskType.Task1, ReadingTrack.Academic, "prompt", null,
            minWordCount, maxWordCount, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_WORD_COUNT_BOUNDS_INVALID", result.Error.Code);
    }
}
