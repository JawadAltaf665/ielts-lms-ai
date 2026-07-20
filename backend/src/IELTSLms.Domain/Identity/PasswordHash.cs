using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// An opaque, already-hashed password credential. The domain never sees plaintext and never hashes
/// or verifies itself — that is the infrastructure hasher's job (Clean Architecture: the domain
/// depends on no crypto library). This type only carries the encoded hash string so the
/// <see cref="User"/> aggregate can hold a credential as a value object (Auth §5).
/// </summary>
public sealed class PasswordHash : ValueObject
{
    private PasswordHash(string value) => Value = value;

    /// <summary>The encoded hash (algorithm + parameters + salt + digest), safe to persist.</summary>
    public string Value { get; }

    /// <summary>Wraps an encoded hash produced by the password hasher.</summary>
    public static PasswordHash FromHashed(string encodedHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedHash);
        return new PasswordHash(encodedHash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
