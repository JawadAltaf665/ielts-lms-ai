using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Abstractions.Context;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Foundation authorization check: verifies the current user satisfies role requirements
/// declared via <see cref="IAuthorizeRequest"/>. Full RBAC/ABAC comes in a later sprint.
/// </summary>
public sealed partial class AuthorizationPipelineBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    ILogger<AuthorizationPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly Error Unauthorized = Error.Custom(
        "AUTHZ_INSUFFICIENT_ROLE",
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
        if (requiredRoles.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        if (currentUser.UserId is null)
        {
            AuthorizationDenied(logger, typeof(TRequest).Name, "anonymous");
            return CreateFailureResult();
        }

        var userRoles = currentUser.Roles;
        var hasRole = requiredRoles.Any(role => userRoles.Contains(role));

        if (!hasRole)
        {
            AuthorizationDenied(logger, typeof(TRequest).Name, currentUser.UserId.Value.ToString());
            return CreateFailureResult();
        }

        return await next().ConfigureAwait(false);
    }

    private static TResponse CreateFailureResult()
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(Unauthorized);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [Unauthorized])!;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Authorization denied for {RequestName} by user {UserId}")]
    private static partial void AuthorizationDenied(ILogger logger, string requestName, string userId);
}
