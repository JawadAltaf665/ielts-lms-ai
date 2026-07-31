using System.Security.Cryptography;
using System.Text;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Time;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.Storage;

/// <summary>
/// Local-disk <see cref="IFileStorage"/> — this codebase has no cloud object-storage account yet, so
/// files are written under <see cref="StorageOptions.RootPath"/> and "signed access" is implemented
/// the same way a pre-signed S3/Blob URL works: <see cref="GetSignedUrlAsync"/> returns a URL carrying
/// an HMAC over the path and an expiry, which <see cref="ValidateSignedUrl"/> re-checks — the file's
/// physical storage key is never exposed or directly reachable without a valid, unexpired signature
/// (API Design Standards §14.1). Swapping this for a real cloud provider later only requires a new
/// <see cref="IFileStorage"/> implementation; nothing above this layer changes.
/// </summary>
public sealed class LocalFileStorage(IOptions<StorageOptions> options, IClock clock) : IFileStorage
{
    private readonly StorageOptions _options = options.Value;

    public async Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(fileStream, ct).ConfigureAwait(false);

        return path;
    }

    public Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(path);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        File.Delete(ResolvePath(path));
        return Task.CompletedTask;
    }

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        var expiresAt = clock.UtcNow.Add(expiry).ToUnixTimeSeconds();
        var signature = Sign(path, expiresAt);

        var url = $"{_options.StreamPathPrefix}?key={Uri.EscapeDataString(path)}&expires={expiresAt}&sig={signature}";
        return Task.FromResult(url);
    }

    public bool ValidateSignedUrl(string path, long expiresAtUnixSeconds, string signature)
    {
        if (clock.UtcNow.ToUnixTimeSeconds() > expiresAtUnixSeconds)
        {
            return false;
        }

        var expected = Sign(path, expiresAtUnixSeconds);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }

    private string Sign(string path, long expiresAtUnixSeconds)
    {
        if (string.IsNullOrEmpty(_options.SigningKey))
        {
            throw new InvalidOperationException(
                "Storage:SigningKey is not configured. Set it before issuing or validating signed file URLs.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{path}|{expiresAtUnixSeconds}"));
        return Convert.ToHexString(hash);
    }

    /// <summary>Resolves a storage-relative path under the configured root, rejecting any attempt to escape it.</summary>
    private string ResolvePath(string path)
    {
        var root = Path.GetFullPath(_options.RootPath);
        var fullPath = Path.GetFullPath(Path.Combine(root, path));

        if (!fullPath.StartsWith(root, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved storage path escapes the configured storage root.");
        }

        return fullPath;
    }
}
