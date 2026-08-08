using IELTSLms.Abstractions.Persistence;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity.Profile;

/// <summary>
/// Handles <see cref="GetCurrentUserQuery"/> by loading the caller's account and projecting the
/// owner-safe profile, including their effective roles and the permissions those roles resolve to
/// (Auth §9). A missing account (e.g. deleted after the token was issued) is reported as a generic
/// unauthorized error rather than a 404, so a valid token never leaks account existence.
/// </summary>
public sealed class GetCurrentUserQueryHandler(
    IUserRepository users, IUserRoleRepository userRoles, IRolePermissionRepository rolePermissions)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(UserId.From(request.UserId), cancellationToken).ConfigureAwait(false);
        if (user is null || user.IsDeleted)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.InvalidCredentials);
        }

        var roles = await userRoles.GetRoleNamesForUserAsync(user.Id.Value, user.TenantId, cancellationToken)
            .ConfigureAwait(false);
        var permissions = await rolePermissions
            .GetPermissionKeysForRoleNamesAsync(roles, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new CurrentUserResponse(
            user.Id.Value,
            user.TenantId,
            user.Email.Value,
            user.DisplayName,
            user.Status.ToString(),
            user.IsEmailVerified,
            user.CreatedAtUtc,
            user.LastLoginAtUtc,
            roles,
            permissions));
    }
}
