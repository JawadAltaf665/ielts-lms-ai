using IELTSLms.Domain.Reading;

namespace IELTSLms.Domain.Tests.Reading;

public sealed class ReadingPassageTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), null, "Global Passage", "Some text", ReadingTrack.Academic,
            ReadingDifficulty.Intermediate, null, 3600, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_tenant_is_not_global()
    {
        var result = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), Guid.NewGuid(), "Tenant Passage", "Some text", ReadingTrack.General,
            ReadingDifficulty.Beginner, null, 1800, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string title)
    {
        var result = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), Guid.NewGuid(), title, "Some text", ReadingTrack.Academic,
            ReadingDifficulty.Beginner, null, 1800, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_TITLE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_rejects_blank_passage_text()
    {
        var result = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", "", ReadingTrack.Academic,
            ReadingDifficulty.Beginner, null, 1800, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_PASSAGE_TEXT_REQUIRED", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_duration(int duration)
    {
        var result = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", "Some text", ReadingTrack.Academic,
            ReadingDifficulty.Beginner, null, duration, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_DURATION_INVALID", result.Error.Code);
    }

    [Fact]
    public void Update_changes_content_and_stamps_updated_at()
    {
        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", "Text", ReadingTrack.Academic,
            ReadingDifficulty.Beginner, null, 1800, Now).Value;
        var updatedAt = Now.AddDays(1);

        var result = passage.Update("New Title", "New text", ReadingTrack.General, ReadingDifficulty.Advanced, null, 2400, updatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", passage.Title);
        Assert.Equal(2400, passage.DurationSeconds);
        Assert.Equal(updatedAt, passage.UpdatedAtUtc);
    }
}
