namespace IELTSLms.Application.Abstractions.Authorization;

/// <summary>
/// Resolves the current caller's effective permission keys from their roles (Auth §9.2, §10.1):
/// role → role-permission grants, within the current tenant. Kept separate from
/// <see cref="IELTSLms.Abstractions.Context.ICurrentUserService"/> because permission resolution
/// needs a database round trip while <c>ICurrentUserService</c>'s other members are cheap,
/// synchronous claim reads.
/// </summary>
public interface IPermissionResolver
{
    /// <summary>The permission keys granted, in the current tenant, to any of the caller's roles.</summary>
    Task<IReadOnlyList<string>> GetPermissionsAsync(CancellationToken cancellationToken = default);
}
