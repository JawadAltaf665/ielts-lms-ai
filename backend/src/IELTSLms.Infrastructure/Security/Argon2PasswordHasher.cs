using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using IELTSLms.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Argon2id password hasher (Auth §5, Security §6): memory-hard, per-hash random salt, constant-time
/// verification. Produces a self-describing PHC-style encoded string
/// (<c>$argon2id$v=19$m=..,t=..,p=..$salt$hash</c>) so parameters travel with the hash and
/// <see cref="Verify"/> can detect out-of-date work factors and request a rehash.
/// </summary>
public sealed class Argon2PasswordHasher(IOptions<Argon2Options> options) : IPasswordHasher
{
    private const string AlgorithmName = "argon2id";
    private const int Version = 19;

    private readonly Argon2Options _options = options.Value;

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(_options.SaltSizeBytes);
        var hash = Derive(password, salt, _options.MemorySizeKib, _options.Iterations, _options.DegreeOfParallelism, _options.HashSizeBytes);

        return string.Create(CultureInfo.InvariantCulture, $"${AlgorithmName}$v={Version}$m={_options.MemorySizeKib},t={_options.Iterations},p={_options.DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    /// <inheritdoc />
    public PasswordVerificationResult Verify(string password, string encodedHash)
    {
        if (string.IsNullOrEmpty(password) || !TryParse(encodedHash, out var parsed))
        {
            return new PasswordVerificationResult(IsMatch: false, NeedsRehash: false);
        }

        var computed = Derive(
            password,
            parsed.Salt,
            parsed.MemorySizeKib,
            parsed.Iterations,
            parsed.DegreeOfParallelism,
            parsed.Hash.Length);

        var isMatch = CryptographicOperations.FixedTimeEquals(computed, parsed.Hash);

        // Only signal a rehash on a genuine match, so a wrong password never triggers a re-hash path.
        var needsRehash = isMatch && (
            parsed.MemorySizeKib != _options.MemorySizeKib ||
            parsed.Iterations != _options.Iterations ||
            parsed.DegreeOfParallelism != _options.DegreeOfParallelism ||
            parsed.Hash.Length != _options.HashSizeBytes);

        return new PasswordVerificationResult(isMatch, needsRehash);
    }

    private static byte[] Derive(string password, byte[] salt, int memoryKib, int iterations, int parallelism, int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKib,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };

        return argon2.GetBytes(hashSize);
    }

    private static bool TryParse(string? encodedHash, out ParsedHash parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(encodedHash))
        {
            return false;
        }

        // Format: $argon2id$v=19$m=<kib>,t=<iter>,p=<par>$<b64 salt>$<b64 hash>
        var parts = encodedHash.Split('$', StringSplitOptions.None);
        if (parts.Length != 6 || parts[0].Length != 0 ||
            !string.Equals(parts[1], AlgorithmName, StringComparison.Ordinal))
        {
            return false;
        }

        var perf = parts[3].Split(',');
        if (perf.Length != 3)
        {
            return false;
        }

        if (!TryReadInt(perf[0], "m=", out var memoryKib) ||
            !TryReadInt(perf[1], "t=", out var iterations) ||
            !TryReadInt(perf[2], "p=", out var parallelism))
        {
            return false;
        }

        try
        {
            parsed = new ParsedHash(
                memoryKib,
                iterations,
                parallelism,
                Convert.FromBase64String(parts[4]),
                Convert.FromBase64String(parts[5]));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryReadInt(string segment, string prefix, out int value)
    {
        value = 0;
        return segment.StartsWith(prefix, StringComparison.Ordinal) &&
            int.TryParse(segment.AsSpan(prefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private readonly record struct ParsedHash(
        int MemorySizeKib,
        int Iterations,
        int DegreeOfParallelism,
        byte[] Salt,
        byte[] Hash);
}
