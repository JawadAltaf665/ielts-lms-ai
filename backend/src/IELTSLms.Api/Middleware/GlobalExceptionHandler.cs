using IELTSLms.Api.Constants;
using IELTSLms.Application.Exceptions;
using IELTSLms.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Last-resort handler for unhandled exceptions. Maps typed exceptions to appropriate HTTP status
/// codes with RFC 7807 <c>application/problem+json</c> responses. Never exposes stack traces,
/// secrets, or cross-tenant details (API Design Standards §9, Security Standards §NFR-SEC).
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

        var (statusCode, code, detail) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        var (type, title) = ProblemTypes.ForStatusCode(statusCode);

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };

        problemDetails.Extensions["code"] = code;
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

    private static (int StatusCode, string Code, string Detail) MapException(Exception exception) =>
        exception switch
        {
            NotFoundException nf => (StatusCodes.Status404NotFound, nf.Code, nf.Message),
            ForbiddenException fb => (StatusCodes.Status403Forbidden, fb.Code, fb.Message),
            UnauthorizedException ua => (StatusCodes.Status401Unauthorized, ua.Code, ua.Message),
            ConflictException cf => (StatusCodes.Status409Conflict, cf.Code, cf.Message),
            ConcurrencyConflictException cc => (StatusCodes.Status409Conflict, cc.Code, cc.Message),
            BusinessRuleException br => (StatusCodes.Status422UnprocessableEntity, br.Code, br.Message),
            DomainException de => (StatusCodes.Status422UnprocessableEntity, de.Code, de.Message),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "CLIENT_CLOSED", "The request was cancelled."),
            _ => (StatusCodes.Status500InternalServerError, "SERVER_UNEXPECTED", "An unexpected error occurred while processing your request."),
        };

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
