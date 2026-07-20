using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Configuration for JWT access-token signing and the parameters the API validates on every request
/// (Auth §3.2, §9; Security Standards §5.1). Signing is asymmetric: the private key mints tokens here
/// and only the public key is needed to verify them. Keys are supplied as PEM so they can come from a
/// secret store without a keystore file.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (<c>iss</c>) — validated on every request.</summary>
    [Required]
    public string Issuer { get; set; } = "ielts-lms-ai";

    /// <summary>Token audience (<c>aud</c>) — validated on every request.</summary>
    [Required]
    public string Audience { get; set; } = "ielts-lms-ai";

    /// <summary>
    /// Signing algorithm; asymmetric only (RS256/ES256). <c>alg:none</c> and symmetric algorithms are
    /// rejected by construction — the validator pins this exact value (Security Standards §5.1).
    /// </summary>
    [Required]
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// PEM-encoded private key used to sign tokens (RSA or EC, matching <see cref="Algorithm"/>).
    /// Optional for local development and tests: when left blank, an ephemeral key is generated at
    /// startup (with a warning). A real key MUST be configured in any deployed environment.
    /// </summary>
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// PEM-encoded public key used to validate tokens. Optional under the same dev-only ephemeral-key
    /// fallback as <see cref="PrivateKeyPem"/>; required in any deployed environment.
    /// </summary>
    public string PublicKeyPem { get; set; } = string.Empty;

    /// <summary>Optional key identifier (<c>kid</c>) stamped in the token header to support rotation.</summary>
    public string? KeyId { get; set; }

    /// <summary>Allowed clock skew when validating token lifetimes. Kept small; defaults to 30 seconds.</summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromSeconds(30);
}
