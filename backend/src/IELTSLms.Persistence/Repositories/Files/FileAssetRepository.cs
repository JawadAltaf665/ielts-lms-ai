using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Files;

internal sealed class FileAssetRepository(ApplicationDbContext dbContext)
    : RepositoryBase<FileAsset, FileAssetId>(dbContext), IFileAssetRepository
{
    public async Task<FileAsset?> GetByStorageKeyAsync(string storageKey, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(f => f.StorageKey == storageKey, cancellationToken).ConfigureAwait(false);
}
