using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Application.Tests.Identity.Fakes;

internal sealed class FakeUserRoleRepository : IUserRoleRepository
{
    private readonly List<UserRole> _assignments = [];
    private readonly Func<RoleId, string?> _roleName;

    /// <param name="roleName">Resolves a RoleId to its Name, mirroring the real repository's join to `roles`.</param>
    public FakeUserRoleRepository(Func<RoleId, string?> roleName) => _roleName = roleName;

    public IReadOnlyList<UserRole> Assignments => _assignments;

    public Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> names = _assignments
            .Where(a => a.UserId.Value == userId && a.TenantId == tenantId)
            .Select(a => _roleName(a.RoleId))
            .Where(n => n is not null)
            .Select(n => n!)
            .Distinct()
            .ToList();
        return Task.FromResult(names);
    }

    public Task<bool> ExistsAsync(Guid userId, RoleId roleId, Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_assignments.Any(a => a.UserId.Value == userId && a.RoleId == roleId && a.TenantId == tenantId));

    public void Add(UserRole userRole) => _assignments.Add(userRole);
}
