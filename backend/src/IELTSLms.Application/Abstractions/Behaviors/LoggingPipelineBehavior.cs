using System.Diagnostics;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Structured request logging around every handler: a start/finish pair with elapsed time and, on
/// failure, the returned error code. Uses structured properties (no string interpolation) so logs
/// stay queryable and never leak PII (Development Standards §12, Observability §3).
/// </summary>
/// <typeparam name="TRequest">The request being handled.</typeparam>
/// <typeparam name="TResponse">The handler response (a <see cref="Result"/> or <c>Result&lt;T&gt;</c>).</typeparam>
public sealed partial class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        RequestStarted(logger, requestName);

        var timestamp = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var elapsedMs = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;

        if (response.IsSuccess)
        {
            RequestCompleted(logger, requestName, elapsedMs);
        }
        else
        {
            RequestFailed(logger, requestName, response.Error.Code, response.Error.Type.ToString(), elapsedMs);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void RequestStarted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs:0.0} ms")]
    private static partial void RequestCompleted(ILogger logger, string requestName, double elapsedMs);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "{RequestName} failed with {ErrorCode} ({ErrorType}) in {ElapsedMs:0.0} ms")]
    private static partial void RequestFailed(
        ILogger logger,
        string requestName,
        string errorCode,
        string errorType,
        double elapsedMs);
}
