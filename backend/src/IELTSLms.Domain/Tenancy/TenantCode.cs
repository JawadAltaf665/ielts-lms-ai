using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// A short, human-facing tenant code (e.g. <c>ACME</c>) used in admin tooling and references. It is
/// normalized to upper-case and constrained to an alphanumeric-with-dashes shape so it is stable and
/// unambiguous across boundaries (Multi-Tenant Architecture §5, ERD ENT-001).
/// </summary>
public sealed class TenantCode : ValueObject
{
    /// <summary>Minimum code length.</summary>
    public const int MinLength = 2;

    /// <summary>Upper bound on stored code length, aligned to the persistence column.</summary>
    public const int MaxLength = 32;

    private TenantCode(string value) => Value = value;

    /// <summary>The normalized code (trimmed, upper-cased using the invariant culture).</summary>
    public string Value { get; }

    /// <summary>
    /// Validates and normalizes a raw code. Returns a validation <see cref="Error"/> instead of
    /// throwing so callers surface a structured failure (Development Standards §10).
    /// </summary>
    public static Result<TenantCode> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Tenant code is required.");
        }

        var normalized = input.Trim().ToUpperInvariant();

        if (normalized.Length is < MinLength or > MaxLength)
        {
            return Error.Validation(
                "VALIDATION_TENANT_CODE_LENGTH",
                $"Tenant code must be between {MinLength} and {MaxLength} characters.");
        }

        if (!IsWellFormed(normalized))
        {
            return Error.Validation(
                "VALIDATION_TENANT_CODE_INVALID",
                "Tenant code may contain only letters, digits, and single dashes between characters.");
        }

        return new TenantCode(normalized);
    }

    /// <summary>Rehydrates a code known to be valid (e.g. from the database) without re-validating.</summary>
    public static TenantCode FromTrusted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    private static bool IsWellFormed(string value)
    {
        if (value[0] == '-' || value[^1] == '-')
        {
            return false;
        }

        var previousWasDash = false;
        foreach (var ch in value)
        {
            if (ch == '-')
            {
                if (previousWasDash)
                {
                    return false;
                }

                previousWasDash = true;
                continue;
            }

            if (!char.IsAsciiLetterOrDigit(ch))
            {
                return false;
            }

            previousWasDash = false;
        }

        return true;
    }
}
