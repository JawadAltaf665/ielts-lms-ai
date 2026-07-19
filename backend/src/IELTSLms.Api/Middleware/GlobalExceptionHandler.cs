using IELTSLms.Api.Constants;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Last-resort handler for unhandled exceptions. Converts them into an RFC 7807
/// <c>application/problem+json</c> response with a generic message — never a stack trace, secret,
/// or any cross-tenant detail — while logging the full exception server-side with the correlation
/// id (API Design Standards §9, Security Standards §NFR-SEC).
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Items.TryGetValue(HeaderNames.CorrelationId, out var value)
            ? value?.ToString()
            : null;

        Log.UnhandledException(
            logger,
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var (type, title) = ProblemTypes.ForErrorType(SharedKernel.Results.ErrorType.Failure);

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred while processing your request.",
            Instance = httpContext.Request.Path,
        };

        problemDetails.Extensions["code"] = "SERVER_UNEXPECTED";
        if (correlationId is not null)
        {
            problemDetails.Extensions["correlationId"] = correlationId;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        }).ConfigureAwait(false);
    }

    /// <summary>High-performance, source-generated log messages for this handler (CA1848).</summary>
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1000,
            Level = LogLevel.Error,
            Message = "Unhandled exception processing {Method} {Path}")]
        public static partial void UnhandledException(
            ILogger logger,
            string method,
            string path,
            Exception exception);
    }
}
