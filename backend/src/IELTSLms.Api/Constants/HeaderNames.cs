namespace IELTSLms.Api.Constants;

/// <summary>
/// Well-known HTTP header names used across the API surface (API Design Standards §10, §12).
/// </summary>
internal static class HeaderNames
{
    /// <summary>Client-supplied or server-generated correlation id, echoed on the response.</summary>
    public const string CorrelationId = "X-Correlation-Id";

    /// <summary>Unique, server-generated id for a single request.</summary>
    public const string RequestId = "X-Request-Id";
}
