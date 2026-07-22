using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

public interface IPermissionRepository : IRepository<Permission, PermissionId>
{
    Task<Permission?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
