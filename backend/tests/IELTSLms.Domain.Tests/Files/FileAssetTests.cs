using IELTSLms.Domain.Files;

namespace IELTSLms.Domain.Tests.Files;

public sealed class FileAssetTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly string[] AllowedTypes = ["audio/mpeg", "audio/wav"];

    [Fact]
    public void Create_accepts_an_allowed_content_type_within_the_size_cap()
    {
        var result = FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "listening-audio/x.mp3", "track.mp3", "audio/mpeg",
            1024, AllowedTypes, maxSizeBytes: 2048, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("audio/mpeg", result.Value.ContentType);
    }

    [Fact]
    public void Create_rejects_a_content_type_outside_the_allowlist()
    {
        var result = FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "listening-audio/x.exe", "malware.exe", "application/x-msdownload",
            1024, AllowedTypes, maxSizeBytes: 2048, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_FILE_CONTENT_TYPE_NOT_ALLOWED", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4096)]
    public void Create_rejects_a_size_outside_the_valid_range(long sizeBytes)
    {
        var result = FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "listening-audio/x.mp3", "track.mp3", "audio/mpeg",
            sizeBytes, AllowedTypes, maxSizeBytes: 2048, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_FILE_SIZE_INVALID", result.Error.Code);
    }

    [Fact]
    public void Create_rejects_a_blank_storage_key()
    {
        var result = FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "  ", "track.mp3", "audio/mpeg",
            1024, AllowedTypes, maxSizeBytes: 2048, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_FILE_STORAGE_KEY_REQUIRED", result.Error.Code);
    }
}
