using System.Text.RegularExpressions;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Tenancy;

/// <summary>
/// A URL-safe, globally-unique tenant identifier used for host-based resolution
/// (<c>acme.ielts-lms.ai</c>) and human-friendly references (Multi-Tenant Architecture §5.1, §8).
/// Normalized to lower-case; constrained to DNS-label-friendly characters so it can back a subdomain.
/// </summary>
public sealed partial class TenantSlug : ValueObject
{
    /// <summary>Lower bound on slug length.</summary>
    public const int MinLength = 3;

    /// <summary>Upper bound on stored slug length, aligned to the persistence column and a DNS label.</summary>
    public const int MaxLength = 63;

    private TenantSlug(string value) => Value = value;

    /// <summary>The normalized slug (trimmed, lower-cased using the invariant culture).</summary>
    public string Value { get; }

    /// <summary>
    /// Validates and normalizes a raw slug. Returns a validation <see cref="Error"/> instead of
    /// throwing so callers surface a structured failure (Development Standards §10).
    /// </summary>
    public static Result<TenantSlug> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Error.Validation("VALIDATION_REQUIRED", "Tenant slug is required.");
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.Length is < MinLength or > MaxLength)
        {
            return Error.Validation(
                "VALIDATION_SLUG_LENGTH",
                $"Tenant slug must be between {MinLength} and {MaxLength} characters.");
        }

        if (!SlugPattern().IsMatch(normalized))
        {
            return Error.Validation(
                "VALIDATION_SLUG_INVALID",
                "Tenant slug may contain only lower-case letters, digits, and single hyphens, and must not start or end with a hyphen.");
        }

        return new TenantSlug(normalized);
    }

    /// <summary>Rehydrates a slug known to be valid (e.g. from the database) without re-validating.</summary>
    public static TenantSlug FromTrusted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    // Lower-case alphanumeric labels separated by single hyphens (no leading/trailing/double hyphen).
    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
