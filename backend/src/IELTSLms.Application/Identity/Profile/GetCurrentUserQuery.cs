using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Profile;

/// <summary>Returns the authenticated caller's profile (Auth §9, "get current user").</summary>
/// <param name="UserId">The authenticated user, from the access-token subject.</param>
public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<CurrentUserResponse>;

/// <summary>The caller's own profile — only non-sensitive fields safe to return to the owner.</summary>
/// <param name="UserId">The stable user identifier.</param>
/// <param name="Email">The account's normalized email.</param>
/// <param name="DisplayName">The user's display name.</param>
/// <param name="Status">The account lifecycle status (e.g. <c>Active</c>).</param>
/// <param name="IsEmailVerified">Whether the account's email has been verified.</param>
/// <param name="CreatedAtUtc">When the account was created (UTC).</param>
/// <param name="LastLoginAtUtc">When the user last signed in (UTC), if ever.</param>
public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Status,
    bool IsEmailVerified,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc);
