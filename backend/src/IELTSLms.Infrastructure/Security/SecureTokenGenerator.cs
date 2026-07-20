using System.Security.Cryptography;
using IELTSLms.Abstractions.Security;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Generates high-entropy opaque tokens (refresh, email-verification, password-reset) using a
/// cryptographically secure RNG, and hashes them with SHA-256 for storage (Auth §3.1, §5). The raw
/// value is URL-safe base64 so it survives being placed in a link or cookie; only the hash is ever
/// persisted. SHA-256 is appropriate here because the input is already high-entropy random — unlike
/// passwords, it needs no slow/memory-hard function.
/// </summary>
public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int TokenSizeBytes = 32;

    /// <inheritdoc />
    public SecureToken Generate()
    {
        var raw = RandomNumberGenerator.GetBytes(TokenSizeBytes);
        var rawValue = Base64UrlEncode(raw);
        return new SecureToken(rawValue, Hash(rawValue));
    }

    /// <inheritdoc />
    public string Hash(string rawToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(rawToken);

        var digest = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(digest);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
