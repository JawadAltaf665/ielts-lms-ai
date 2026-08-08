using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;

namespace IELTSLms.Application.Abstractions.Authorization;

/// <summary>
/// Default <see cref="IPermissionResolver"/>: takes the caller's role names straight off the
/// (already-validated) token via <see cref="ICurrentUserService"/> — no extra round trip to
/// re-derive them — and looks up the permission keys those roles grant.
/// </summary>
/// <remarks>
/// <c>ponytail:</c> one query per authorized request, no caching. Fine at current scale; if this
/// becomes hot, cache per sorted-role-set with a short TTL or invalidate on grant change
/// (Multi-Tenant Architecture §10.1 Redis strategy already exists for this shape of problem).
/// </remarks>
internal sealed class PermissionResolver(ICurrentUserService currentUser, IRolePermissionRepository rolePermissions)
    : IPermissionResolver
{
    public async Task<IReadOnlyList<string>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.Roles.Count == 0)
        {
            return [];
        }

        return await rolePermissions
            .GetPermissionKeysForRoleNamesAsync(currentUser.Roles, cancellationToken)
            .ConfigureAwait(false);
    }
}
