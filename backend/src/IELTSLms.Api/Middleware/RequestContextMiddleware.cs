using IELTSLms.Api.Constants;
using Serilog.Context;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Enriches the request context with additional properties and pushes them into the Serilog LogContext.
/// Runs after CorrelationIdMiddleware and before authentication.
/// </summary>
internal sealed class RequestContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var requestId = context.TraceIdentifier;

        using (LogContext.PushProperty("ClientIP", clientIp ?? "unknown"))
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserAgent", Truncate(userAgent, 256)))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
