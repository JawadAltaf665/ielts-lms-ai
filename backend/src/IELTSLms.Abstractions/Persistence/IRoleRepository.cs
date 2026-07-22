using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

public interface IRoleRepository : IRepository<Role, RoleId>
{
    Task<Role?> GetByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
