using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Domain.Files;

namespace IELTSLms.Application.Tests.Listening.Fakes;

internal sealed class FakeFileAssetRepository : IFileAssetRepository
{
    private readonly List<FileAsset> _files = [];

    public void Seed(FileAsset file) => _files.Add(file);

    public IReadOnlyList<FileAsset> Files => _files;

    public Task<FileAsset?> GetByIdAsync(FileAssetId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_files.FirstOrDefault(f => f.Id == id));

    public void Add(FileAsset aggregate) => _files.Add(aggregate);

    public void Remove(FileAsset aggregate) => _files.Remove(aggregate);

    public Task<FileAsset?> GetByStorageKeyAsync(string storageKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(_files.FirstOrDefault(f => f.StorageKey == storageKey));
}
