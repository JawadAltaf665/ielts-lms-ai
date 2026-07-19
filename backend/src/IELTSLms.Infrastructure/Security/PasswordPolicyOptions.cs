using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Configuration for <see cref="ConfigurablePasswordPolicy"/> (Auth §5, Security §6). Defaults meet
/// the standard's floor: a 12-character minimum with mixed character classes.
/// </summary>
public sealed class PasswordPolicyOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "PasswordPolicy";

    /// <summary>Minimum length; the standard floor is 12 (Security §6).</summary>
    [Range(12, 512)]
    public int MinimumLength { get; set; } = 12;

    /// <summary>Upper bound to bound hashing cost and reject pathological inputs.</summary>
    [Range(12, 1024)]
    public int MaximumLength { get; set; } = 256;

    /// <summary>Require at least one uppercase letter.</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>Require at least one lowercase letter.</summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>Require at least one digit.</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>Require at least one non-alphanumeric character.</summary>
    public bool RequireNonAlphanumeric { get; set; } = true;
}
