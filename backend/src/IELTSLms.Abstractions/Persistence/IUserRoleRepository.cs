using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Access to <see cref="UserRole"/> assignments (ENT-008). Not an <c>IRepository</c> — the join
/// entity has no aggregate identity of its own worth loading by id; callers work in terms of a
/// user's role names within a tenant (Auth §9.1 — JWT <c>role</c> claims, <c>/auth/me</c>).
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>The names of every role assigned to <paramref name="userId"/> within <paramref name="tenantId"/>.</summary>
    Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Whether the user already holds this role in this tenant (assignment idempotency).</summary>
    Task<bool> ExistsAsync(
        Guid userId, RoleId roleId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new assignment to be persisted on the next unit-of-work commit.</summary>
    void Add(UserRole userRole);
}
