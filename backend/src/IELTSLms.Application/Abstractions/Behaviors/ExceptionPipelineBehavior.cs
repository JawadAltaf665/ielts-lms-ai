using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Catches unhandled exceptions from downstream handlers and maps them to Result.Failure.
/// OperationCanceledException propagates (caller cancellation is not an error).
/// </summary>
public sealed partial class ExceptionPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            UnhandledExceptionInHandler(logger, typeof(TRequest).Name, ex);
            return CreateFailureResult(ex);
        }
    }

    private static TResponse CreateFailureResult(Exception ex)
    {
        var error = Error.Failure("SERVER_UNEXPECTED", ex.Message);

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

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unhandled exception in handler {RequestName}")]
    private static partial void UnhandledExceptionInHandler(
        ILogger logger, string requestName, Exception exception);
}
