using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeRoleRepository : IRoleRepository
{
    private readonly Dictionary<RoleId, Role> _byId = [];

    public void Seed(Role role) => _byId[role.Id] = role;

    public Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public void Add(Role aggregate) => _byId[aggregate.Id] = aggregate;

    public void Remove(Role aggregate) => _byId.Remove(aggregate.Id);

    public Task<Role?> GetByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.FirstOrDefault(r => r.TenantId == tenantId && r.Name == name));

    public Task<List<Role>> GetRolesForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_byId.Values.Where(r => r.TenantId == tenantId || r.TenantId == null).ToList());
}
