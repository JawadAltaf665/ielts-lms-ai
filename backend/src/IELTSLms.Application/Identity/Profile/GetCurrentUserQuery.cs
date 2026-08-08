using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Profile;

/// <summary>Returns the authenticated caller's profile (Auth §9, "get current user").</summary>
/// <param name="UserId">The authenticated user, from the access-token subject.</param>
public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<CurrentUserResponse>;

/// <summary>
/// The caller's own profile — non-sensitive fields safe to return to the owner, plus their
/// effective authorization context (Gap 2: roles/permissions were previously omitted entirely).
/// </summary>
/// <param name="UserId">The stable user identifier.</param>
/// <param name="TenantId">The tenant this account belongs to.</param>
/// <param name="Email">The account's normalized email.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Status">The account lifecycle status (e.g. <c>Active</c>).</param>
/// <param name="IsEmailVerified">Whether the account's email has been verified.</param>
/// <param name="CreatedAtUtc">When the account was created (UTC).</param>
/// <param name="LastLoginAtUtc">When the user last signed in (UTC), if ever.</param>
/// <param name="Roles">Assigned role names within the tenant (same set as the token's <c>role</c> claims).</param>
/// <param name="Permissions">
/// The permission keys those roles resolve to (Auth §9.2) — UI convenience only; the backend
/// remains authoritative and re-checks on every request (Auth §10.2).
/// </param>
public sealed record CurrentUserResponse(
    Guid UserId,
    Guid TenantId,
    string Email,
    string DisplayName,
    string Status,
    bool IsEmailVerified,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
