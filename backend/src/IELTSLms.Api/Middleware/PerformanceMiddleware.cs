using System.Diagnostics;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Tracks request duration and logs a warning for requests exceeding the configured threshold.
/// </summary>
internal sealed partial class PerformanceMiddleware(
    RequestDelegate next,
    ILogger<PerformanceMiddleware> logger)
{
    private const long DefaultThresholdMs = 500;

    public async Task InvokeAsync(HttpContext context)
    {
        var timestamp = Stopwatch.GetTimestamp();

        await next(context).ConfigureAwait(false);

        var elapsedMs = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;

        if (elapsedMs > DefaultThresholdMs)
        {
            SlowRequest(logger, context.Request.Method, context.Request.Path, elapsedMs);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Slow request: {Method} {Path} took {ElapsedMs:0.0} ms")]
    private static partial void SlowRequest(ILogger logger, string method, string path, double elapsedMs);
}
