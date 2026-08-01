using IELTSLms.Abstractions.Storage;

namespace IELTSLms.Application.Tests.Certificates.Fakes;

internal sealed class FakeFileStorage : IFileStorage
{
    public int UploadCallCount { get; private set; }

    public Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default)
    {
        UploadCallCount++;
        return Task.FromResult(path);
    }

    public Task<Stream> DownloadAsync(string path, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream());

    public Task DeleteAsync(string path, CancellationToken ct = default) => Task.CompletedTask;

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default) =>
        Task.FromResult($"https://example.test/{path}");

    public bool ValidateSignedUrl(string path, long expiresAtUnixSeconds, string signature) => true;
}
