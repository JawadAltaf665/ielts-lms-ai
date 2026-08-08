using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Abstractions.Context;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// RBAC + permission-based authorization (Auth §10.1): verifies the current user satisfies both the
/// role and permission requirements declared via <see cref="IAuthorizeRequest"/>. Role checks are
/// a cheap claim comparison; permission checks resolve the caller's effective permissions from their
/// roles via <see cref="IPermissionResolver"/> (one role→permission lookup, only when the request
/// actually declares <see cref="IAuthorizeRequest.RequiredPermissions"/>). Policy-based checks
/// (ownership/quota/resource-state) remain the individual handler's responsibility (§10.1 — a third
/// layer this behavior does not attempt to generalize).
/// </summary>
public sealed partial class AuthorizationPipelineBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    IPermissionResolver permissionResolver,
    ILogger<AuthorizationPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly Error UnauthorizedRole = Error.Custom(
        "AUTHZ_INSUFFICIENT_ROLE",
        "You do not have permission to perform this action.",
        ErrorType.Forbidden);

    private static readonly Error UnauthorizedPermission = Error.Custom(
        "AUTHZ_INSUFFICIENT_PERMISSION",
        "You do not have permission to perform this action.",
        ErrorType.Forbidden);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuthorizeRequest authorizeRequest)
        {
            return await next().ConfigureAwait(false);
        }

        var requiredRoles = authorizeRequest.RequiredRoles;
        var requiredPermissions = authorizeRequest.RequiredPermissions;
        if (requiredRoles.Length == 0 && requiredPermissions.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        if (currentUser.UserId is null)
        {
            AuthorizationDenied(logger, typeof(TRequest).Name, "anonymous", "no-user");
            return CreateFailureResult(UnauthorizedRole);
        }

        if (requiredRoles.Length > 0)
        {
            var userRoles = currentUser.Roles;
            if (!requiredRoles.Any(role => userRoles.Contains(role)))
            {
                AuthorizationDenied(logger, typeof(TRequest).Name, currentUser.UserId.Value.ToString(), "role");
                return CreateFailureResult(UnauthorizedRole);
            }
        }

        if (requiredPermissions.Length > 0)
        {
            var granted = await permissionResolver.GetPermissionsAsync(cancellationToken).ConfigureAwait(false);
            if (!requiredPermissions.Any(granted.Contains))
            {
                AuthorizationDenied(logger, typeof(TRequest).Name, currentUser.UserId.Value.ToString(), "permission");
                return CreateFailureResult(UnauthorizedPermission);
            }
        }

        return await next().ConfigureAwait(false);
    }

    private static TResponse CreateFailureResult(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(error);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Authorization denied for {RequestName} by user {UserId} ({Reason})")]
    private static partial void AuthorizationDenied(ILogger logger, string requestName, string userId, string reason);
}
