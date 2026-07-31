namespace IELTSLms.Abstractions.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>
    /// Verifies a signature previously issued by <see cref="GetSignedUrlAsync"/> for <paramref name="path"/>,
    /// checking both the HMAC and the expiry — the sole gate on the direct-download endpoint (API Design
    /// Standards §14.1: "direct execution is never permitted; access via signed, expiring URLs").
    /// </summary>
    bool ValidateSignedUrl(string path, long expiresAtUnixSeconds, string signature);
}
