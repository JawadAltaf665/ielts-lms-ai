using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A validated, normalized email address. Construction goes through <see cref="Create"/> so an
/// <see cref="Email"/> instance is always well-formed; normalization (trim + lower-case) gives a
/// single canonical form for uniqueness checks (Auth §5, ERD ENT-003).
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>Upper bound on stored address length, aligned to the persistence column.</summary>
    public const int MaxLength = 320;

    private Email(string value) => Value = value;

    /// <summary>The normalized address (trimmed, lower-cased using the invariant culture).</summary>
    public string Value { get; }

    /// <summary>
    /// Validates and normalizes a raw address. Returns a validation <see cref="Error"/> instead of
    /// throwing so callers surface a structured failure (Development Standards §10).
    /// </summary>
    public static Result<Email> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Email is required.");
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            return Error.Validation("VALIDATION_EMAIL_TOO_LONG", $"Email must not exceed {MaxLength} characters.");
        }

        if (!IsWellFormed(normalized))
        {
            return Error.Validation("VALIDATION_EMAIL_INVALID", "Email is not a valid address.");
        }

        return new Email(normalized);
    }

    /// <summary>Rehydrates an email known to be valid (e.g. from the database) without re-validating.</summary>
    public static Email FromTrusted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    private static bool IsWellFormed(string value)
    {
        // Structural check without a permissive regex: exactly one '@', non-empty local part, and a
        // dotted domain with a multi-char TLD. Deliverability is confirmed by the verification flow.
        var atIndex = value.IndexOf('@', StringComparison.Ordinal);
        if (atIndex <= 0 || atIndex != value.LastIndexOf('@'))
        {
            return false;
        }

        var local = value[..atIndex];
        var domain = value[(atIndex + 1)..];

        if (local.Length == 0 || domain.Length < 3 || value.Contains(' ', StringComparison.Ordinal))
        {
            return false;
        }

        var lastDot = domain.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == domain.Length - 1)
        {
            return false;
        }

        var tld = domain[(lastDot + 1)..];
        return tld.Length >= 2 && !domain.StartsWith('.') && !domain.Contains("..", StringComparison.Ordinal);
    }
}
