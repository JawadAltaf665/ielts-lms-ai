using IELTSLms.Application.Files.GetFileContent;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;

namespace IELTSLms.Application.Tests.Files;

/// <summary>Sprint 5 test note: "signed-URL expiry test" — the sole gate on the direct-download route is the HMAC + expiry, not JWT auth.</summary>
public sealed class GetFileContentQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const string StorageKey = "listening-audio/x.mp3";

    private static void SeedFile(FakeFileAssetRepository files) =>
        files.Seed(FileAsset.Create(
            FileAssetId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), StorageKey, "x.mp3", "audio/mpeg", 1024, ["audio/mpeg"], 2048, Now)
            .Value);

    [Fact]
    public async Task A_signature_issued_by_GetSignedUrlAsync_streams_the_file()
    {
        var files = new FakeFileAssetRepository();
        SeedFile(files);
        var storage = new FakeFileStorage(new FakeClock(Now));
        var (key, expires, sig) = await IssueSignedUrl(storage);
        var handler = new GetFileContentQueryHandler(files, storage, new FakeTenantContextAccessor());

        var result = await handler.Handle(new GetFileContentQuery(key, expires, sig), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("audio/mpeg", result.Value.ContentType);
    }

    [Fact]
    public async Task An_expired_signature_is_rejected()
    {
        var files = new FakeFileAssetRepository();
        SeedFile(files);
        var issuingStorage = new FakeFileStorage(new FakeClock(Now));
        var (key, expires, sig) = await IssueSignedUrl(issuingStorage);

        // Validate with a clock past the URL's expiry — mirrors LocalFileStorage.ValidateSignedUrl's own clock check.
        var validatingStorage = new FakeFileStorage(new FakeClock(Now.AddMinutes(16)));
        var handler = new GetFileContentQueryHandler(files, validatingStorage, new FakeTenantContextAccessor());

        var result = await handler.Handle(new GetFileContentQuery(key, expires, sig), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_FILE_SIGNATURE_INVALID", result.Error.Code);
    }

    [Fact]
    public async Task A_tampered_signature_is_rejected()
    {
        var files = new FakeFileAssetRepository();
        SeedFile(files);
        var storage = new FakeFileStorage(new FakeClock(Now));
        var (key, expires, _) = await IssueSignedUrl(storage);
        var handler = new GetFileContentQueryHandler(files, storage, new FakeTenantContextAccessor());

        var result = await handler.Handle(new GetFileContentQuery(key, expires, "tampered-signature"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_FILE_SIGNATURE_INVALID", result.Error.Code);
    }

    private static async Task<(string Key, long Expires, string Sig)> IssueSignedUrl(FakeFileStorage storage)
    {
        var url = await storage.GetSignedUrlAsync(StorageKey, TimeSpan.FromMinutes(15));
        var query = url[(url.IndexOf('?') + 1)..].Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => p[0], p => p[1]);
        return (query["key"], long.Parse(query["expires"], System.Globalization.CultureInfo.InvariantCulture), query["sig"]);
    }
}
