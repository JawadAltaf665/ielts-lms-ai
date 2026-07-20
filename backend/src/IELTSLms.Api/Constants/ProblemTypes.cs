using IELTSLms.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace IELTSLms.Api.Constants;

/// <summary>
/// Maps error categories to their canonical <c>type</c> URI and title for RFC 7807 problem
/// documents (API Design Standards §9.1). The base URI is a stable, dereferenceable namespace.
/// </summary>
internal static class ProblemTypes
{
    private const string BaseUri = "https://errors.ielts-lms.ai/";

    public static (string Type, string Title) ForErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => (BaseUri + "validation", "Validation failed"),
        ErrorType.NotFound => (BaseUri + "not-found", "Resource not found"),
        ErrorType.Conflict => (BaseUri + "conflict", "Conflict"),
        ErrorType.Unauthorized => (BaseUri + "unauthorized", "Authentication required"),
        ErrorType.Forbidden => (BaseUri + "forbidden", "Forbidden"),
        _ => (BaseUri + "server-error", "An unexpected error occurred"),
    };

    public static (string Type, string Title) ForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => (BaseUri + "bad-request", "Bad request"),
        StatusCodes.Status401Unauthorized => (BaseUri + "unauthorized", "Authentication required"),
        StatusCodes.Status403Forbidden => (BaseUri + "forbidden", "Forbidden"),
        StatusCodes.Status404NotFound => (BaseUri + "not-found", "Resource not found"),
        StatusCodes.Status409Conflict => (BaseUri + "conflict", "Conflict"),
        StatusCodes.Status422UnprocessableEntity => (BaseUri + "validation", "Validation failed"),
        StatusCodes.Status429TooManyRequests => (BaseUri + "rate-limit", "Too many requests"),
        StatusCodes.Status499ClientClosedRequest => (BaseUri + "client-closed", "Client closed request"),
        StatusCodes.Status503ServiceUnavailable => (BaseUri + "service-unavailable", "Service unavailable"),
        _ => (BaseUri + "server-error", "An unexpected error occurred"),
    };
}
