namespace IELTSLms.Abstractions.Security;

/// <summary>
/// Generates high-entropy opaque tokens (refresh, email-verification, password-reset) and hashes
/// them for storage (Auth §3.1, §5: tokens stored hashed, never plaintext). Uses a cryptographically
/// secure RNG. Hashing here is a fast one-way digest (SHA-256) suitable for high-entropy random
/// values — distinct from password hashing, which must be slow/memory-hard.
/// </summary>
public interface ISecureTokenGenerator
{
    /// <summary>Creates a new random token: the raw value to send, and its hash to persist.</summary>
    SecureToken Generate();

    /// <summary>Computes the storage hash of a raw token so an incoming value can be looked up.</summary>
    string Hash(string rawToken);
}

/// <summary>A freshly generated secure token.</summary>
/// <param name="RawValue">The opaque value handed to the client; never persisted.</param>
/// <param name="Hash">The digest of <paramref name="RawValue"/> stored server-side for lookup.</param>
public readonly record struct SecureToken(string RawValue, string Hash);
