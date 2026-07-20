using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Context;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Records an activity log entry for every successfully executed command.
/// Queries are read-only and produce no audit trail.
/// </summary>
public sealed partial class AuditPipelineBehavior<TRequest, TResponse>(
    IActivityLogger activityLogger,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService,
    ILogger<AuditPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand())
        {
            return await next().ConfigureAwait(false);
        }

        var response = await next().ConfigureAwait(false);

        if (response.IsSuccess)
        {
            try
            {
                await activityLogger.LogAsync(
                    typeof(TRequest).Name,
                    currentUserService.UserId,
                    currentTenantService.TenantId,
                    ct: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AuditWriteFailed(logger, typeof(TRequest).Name, ex);
            }
        }

        return response;
    }

    private static bool IsCommand()
    {
        return typeof(ICommand).IsAssignableFrom(typeof(TRequest))
            || typeof(TRequest).GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to write audit for {RequestName}")]
    private static partial void AuditWriteFailed(ILogger logger, string requestName, Exception ex);
}
