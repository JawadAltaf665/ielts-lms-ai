using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Domain.Tests.Listening;

public sealed class ListeningAudioTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly FileAssetId FileId = FileAssetId.From(Guid.NewGuid());

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), null, "Global Audio", ReadingTrack.Academic, FileId, null, 600, null, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_tenant_is_not_global()
    {
        var result = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), Guid.NewGuid(), "Tenant Audio", ReadingTrack.General, FileId, null, 600, null, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string title)
    {
        var result = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), Guid.NewGuid(), title, ReadingTrack.Academic, FileId, null, 600, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LISTENING_TITLE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_rejects_an_empty_file_id()
    {
        var result = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", ReadingTrack.Academic, FileAssetId.From(Guid.Empty), null, 600,
            null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LISTENING_FILE_REQUIRED", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_duration(int duration)
    {
        var result = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", ReadingTrack.Academic, FileId, null, duration, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LISTENING_DURATION_INVALID", result.Error.Code);
    }

    [Fact]
    public void Update_changes_metadata_and_stamps_updated_at()
    {
        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), Guid.NewGuid(), "Title", ReadingTrack.Academic, FileId, null, 600, null, Now).Value;
        var updatedAt = Now.AddDays(1);

        var result = audio.Update("New Title", ReadingTrack.General, null, 900, "A transcript.", updatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", audio.Title);
        Assert.Equal(900, audio.DurationSeconds);
        Assert.Equal("A transcript.", audio.TranscriptText);
        Assert.Equal(updatedAt, audio.UpdatedAtUtc);
    }
}
