namespace IELTSLms.SharedKernel.Results;

/// <summary>
/// A single field-level validation failure: which member failed and why.
/// </summary>
/// <param name="PropertyName">The offending member/field name.</param>
/// <param name="Code">Stable machine code for the specific rule that failed.</param>
/// <param name="Message">Human-readable description of the failure.</param>
public sealed record ValidationFailureDetail(string PropertyName, string Code, string Message);

/// <summary>
/// An <see cref="Error"/> of type <see cref="ErrorType.Validation"/> that additionally carries the
/// full set of field-level failures, so the API boundary can emit an RFC 9457 validation problem
/// document with per-field detail (API Design Standards §9).
/// </summary>
public sealed record ValidationError : Error
{
    public ValidationError(IReadOnlyList<ValidationFailureDetail> failures)
        : base("VALIDATION_ERROR", "One or more validation errors occurred.", ErrorType.Validation)
    {
        Failures = failures;
    }

    private ValidationError(string code, string message, IReadOnlyList<ValidationFailureDetail> failures)
        : base(code, message, ErrorType.Validation)
    {
        Failures = failures;
    }

    /// <summary>The individual field failures that make up this validation error.</summary>
    public IReadOnlyList<ValidationFailureDetail> Failures { get; }

    /// <summary>Builds a validation error, using <paramref name="primary"/> for the top-level code/message.</summary>
    public static ValidationError Create(Error primary, IReadOnlyList<ValidationFailureDetail> failures) =>
        new(primary.Code, primary.Message, failures);
}
