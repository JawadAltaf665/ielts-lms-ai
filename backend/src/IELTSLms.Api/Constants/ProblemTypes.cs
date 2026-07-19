using IELTSLms.SharedKernel.Results;

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
}
