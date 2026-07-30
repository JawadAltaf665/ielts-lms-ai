namespace IELTSLms.SharedKernel.Results;

/// <summary>
/// Classifies an <see cref="Error"/> so the API layer can map it to the correct HTTP status
/// (API Design Standards §5.2 / §9.2) without the domain knowing about HTTP.
/// </summary>
public enum ErrorType
{
    /// <summary>Unexpected/technical failure → 500.</summary>
    Failure = 0,

    /// <summary>Input/structural or business-rule validation → 400/422.</summary>
    Validation = 1,

    /// <summary>Resource does not exist (or is hidden cross-tenant) → 404.</summary>
    NotFound = 2,

    /// <summary>State or concurrency conflict → 409.</summary>
    Conflict = 3,

    /// <summary>Authentication missing/invalid → 401.</summary>
    Unauthorized = 4,

    /// <summary>Authenticated but not permitted → 403.</summary>
    Forbidden = 5,

    /// <summary>Caller exceeded a rate limit or quota → 429.</summary>
    RateLimited = 6,

    /// <summary>An upstream dependency is unavailable (outage, circuit-open) → 503.</summary>
    ServiceUnavailable = 7,
}
