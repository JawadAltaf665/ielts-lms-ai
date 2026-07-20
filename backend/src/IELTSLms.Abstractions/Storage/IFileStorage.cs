namespace IELTSLms.Abstractions.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(string path, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default);
}
