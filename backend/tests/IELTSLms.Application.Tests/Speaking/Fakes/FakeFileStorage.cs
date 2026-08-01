using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Time;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

/// <summary>In-memory <see cref="IFileStorage"/> mirroring <c>LocalFileStorage</c>'s sign/validate contract for handler tests.</summary>
internal sealed class FakeFileStorage(IClock clock) : IFileStorage
{
    private readonly Dictionary<string, byte[]> _uploads = [];

    public List<string> DeletedPaths { get; } = [];

    public Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        _uploads[path] = buffer.ToArray();
        return Task.FromResult(path);
    }

    public Task<Stream> DownloadAsync(string path, CancellationToken ct = default) =>
        Task.FromResult<Stream>(new MemoryStream(_uploads.TryGetValue(path, out var bytes) ? bytes : []));

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        DeletedPaths.Add(path);
        _uploads.Remove(path);
        return Task.CompletedTask;
    }

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        var expiresAt = clock.UtcNow.Add(expiry).ToUnixTimeSeconds();
        return Task.FromResult($"/files/stream?key={path}&expires={expiresAt}&sig={Sign(path, expiresAt)}");
    }

    public bool ValidateSignedUrl(string path, long expiresAtUnixSeconds, string signature) =>
        clock.UtcNow.ToUnixTimeSeconds() <= expiresAtUnixSeconds && signature == Sign(path, expiresAtUnixSeconds);

    private static string Sign(string path, long expiresAtUnixSeconds) => $"{path}|{expiresAtUnixSeconds}";
}
