namespace IELTSLms.SharedKernel.Results;

/// <summary>
/// A stable, machine-readable failure descriptor. The <see cref="Code"/> is part of the
/// API contract (API Design Standards §9.2) and never changes meaning once published.
/// </summary>
public record Error
{
    /// <summary>Represents "no error" — the success sentinel used by <see cref="Result"/>.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>Represents an unexpected null value where a result was required.</summary>
    public static readonly Error NullValue = new(
        "GENERAL_NULL_VALUE",
        "A null value was provided where one was not allowed.",
        ErrorType.Failure);

    public Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    /// <summary>Stable machine code, e.g. <c>VALIDATION_ERROR</c>, <c>CONFLICT_VERSION_MISMATCH</c>.</summary>
    public string Code { get; }

    /// <summary>Human-readable, safe-to-surface message. Never contains internals, secrets, or PII.</summary>
    public string Message { get; }

    /// <summary>Category used to map to the correct HTTP status at the API boundary.</summary>
    public ErrorType Type { get; }

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Problem(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error RateLimited(string code, string message) => new(code, message, ErrorType.RateLimited);

    public static Error ServiceUnavailable(string code, string message) => new(code, message, ErrorType.ServiceUnavailable);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Custom(string code, string message, ErrorType type) => new(code, message, type);
}
