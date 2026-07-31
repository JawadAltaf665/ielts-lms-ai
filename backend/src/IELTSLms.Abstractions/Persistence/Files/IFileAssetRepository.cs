using IELTSLms.Domain.Files;

namespace IELTSLms.Abstractions.Persistence.Files;

/// <summary>Aggregate access for <see cref="FileAsset"/> (ERD ENT-057).</summary>
public interface IFileAssetRepository : IRepository<FileAsset, FileAssetId>
{
    /// <summary>Looks up a file by its object-storage key — used by the signed streaming endpoint to resolve content type/name.</summary>
    Task<FileAsset?> GetByStorageKeyAsync(string storageKey, CancellationToken cancellationToken = default);
}
