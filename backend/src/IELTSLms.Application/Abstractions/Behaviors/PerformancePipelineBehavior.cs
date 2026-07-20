using System.Diagnostics;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Logs a warning when a handler exceeds the configured slow-request threshold.
/// </summary>
public sealed partial class PerformancePipelineBehavior<TRequest, TResponse>(
    ILogger<PerformancePipelineBehavior<TRequest, TResponse>> logger,
    IConfiguration configuration)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly long _thresholdMs = configuration.GetValue("Logging:Behavior:SlowRequestThresholdMs", 500);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var timestamp = Stopwatch.GetTimestamp();
        var response = await next().ConfigureAwait(false);
        var elapsedMs = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;

        if (elapsedMs > _thresholdMs)
        {
            SlowRequestDetected(logger, typeof(TRequest).Name, elapsedMs, _thresholdMs);
        }

        return response;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Slow request {RequestName}: {ElapsedMs:0.0} ms (threshold {ThresholdMs} ms)")]
    private static partial void SlowRequestDetected(
        ILogger logger, string requestName, double elapsedMs, long thresholdMs);
}
