using IELTSLms.Api.Constants;
using Serilog.Context;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Establishes a correlation id for every request: reuses a valid client-supplied
/// <c>X-Correlation-Id</c> or generates one, echoes it on the response, and pushes it onto the
/// Serilog <see cref="LogContext"/> so every log line for the request carries it
/// (Observability §3, API Design Standards §10).
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>Max accepted length of a client-supplied correlation id, to bound log field size.</summary>
    private const int MaxCorrelationIdLength = 128;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        // Make it available to downstream components and the exception handler.
        context.Items[HeaderNames.CorrelationId] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderNames.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var provided))
        {
            var candidate = provided.ToString();
            if (!string.IsNullOrWhiteSpace(candidate) && candidate.Length <= MaxCorrelationIdLength)
            {
                return candidate;
            }
        }

        return Guid.CreateVersion7().ToString("n");
    }
}
