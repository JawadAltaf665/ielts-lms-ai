namespace IELTSLms.Abstractions.Security;

/// <summary>
/// Hashes and verifies user passwords with a memory-hard algorithm (Argon2id) and a per-hash salt
/// (Auth §5, Security Standards §6). The domain depends on this port and never on a crypto library
/// (Clean Architecture §5.1). Implementations must compare in constant time to resist timing attacks.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plaintext password, producing a self-describing encoded string to persist.</summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against an encoded hash in constant time.
    /// </summary>
    /// <returns>
    /// A result indicating whether the password matched and whether the stored hash used
    /// out-of-date parameters and should be re-hashed on the next successful login.
    /// </returns>
    PasswordVerificationResult Verify(string password, string encodedHash);
}

/// <summary>Outcome of <see cref="IPasswordHasher.Verify"/>.</summary>
/// <param name="IsMatch">Whether the supplied password matched the stored hash.</param>
/// <param name="NeedsRehash">Whether the stored hash should be upgraded to current parameters.</param>
public readonly record struct PasswordVerificationResult(bool IsMatch, bool NeedsRehash);
