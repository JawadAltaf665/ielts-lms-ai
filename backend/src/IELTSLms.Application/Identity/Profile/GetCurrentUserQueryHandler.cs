using IELTSLms.Abstractions.Persistence;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Identity;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Identity.Profile;

/// <summary>
/// Handles <see cref="GetCurrentUserQuery"/> by loading the caller's account and projecting the
/// owner-safe profile. A missing account (e.g. deleted after the token was issued) is reported as a
/// generic unauthorized error rather than a 404, so a valid token never leaks account existence.
/// </summary>
public sealed class GetCurrentUserQueryHandler(IUserRepository users)
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

        return Result.Success(new CurrentUserResponse(
            user.Id.Value,
            user.Email.Value,
            user.DisplayName,
            user.Status.ToString(),
            user.IsEmailVerified,
            user.CreatedAtUtc,
            user.LastLoginAtUtc));
    }
}
