using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Owns the asymmetric key material for JWT signing and validation, resolved once from
/// <see cref="JwtOptions"/> (Auth §3.2, Security §5.1). Registered as a singleton so the token
/// service (signing) and the API's bearer middleware (validation) share exactly one key. When no PEM
/// is configured — local development and tests only — an ephemeral RSA key is generated so the auth
/// flows run end-to-end without provisioning secrets; a warning is logged because such tokens do not
/// survive a restart and must never be used in a deployed environment.
/// </summary>
public sealed partial class JwtSigningKeyProvider : IDisposable
{
    private readonly RSA? _rsa;
    private readonly ECDsa? _ecdsa;

    public JwtSigningKeyProvider(IOptions<JwtOptions> options, ILogger<JwtSigningKeyProvider> logger)
    {
        var jwt = options.Value;
        Algorithm = jwt.Algorithm;

        if (jwt.Algorithm.StartsWith("ES", StringComparison.Ordinal))
        {
            _ecdsa = ECDsa.Create();
            if (!string.IsNullOrWhiteSpace(jwt.PrivateKeyPem))
            {
                _ecdsa.ImportFromPem(jwt.PrivateKeyPem);
            }
            else
            {
                EphemeralKeyGenerated(logger);
            }

            SigningKey = new ECDsaSecurityKey(_ecdsa) { KeyId = jwt.KeyId };
        }
        else if (jwt.Algorithm.StartsWith("RS", StringComparison.Ordinal))
        {
            _rsa = RSA.Create();
            if (!string.IsNullOrWhiteSpace(jwt.PrivateKeyPem))
            {
                _rsa.ImportFromPem(jwt.PrivateKeyPem);
            }
            else
            {
                _rsa.KeySize = 3072;
                EphemeralKeyGenerated(logger);
            }

            SigningKey = new RsaSecurityKey(_rsa) { KeyId = jwt.KeyId };
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported JWT signing algorithm '{jwt.Algorithm}'. Only asymmetric RS*/ES* algorithms are allowed.");
        }

        ValidationKey = SigningKey;
        SigningCredentials = new SigningCredentials(SigningKey, Algorithm);
    }

    /// <summary>The configured signing algorithm (e.g. <c>RS256</c>).</summary>
    public string Algorithm { get; }

    /// <summary>The private key material used to sign tokens.</summary>
    public SecurityKey SigningKey { get; }

    /// <summary>The public key material used to validate tokens.</summary>
    public SecurityKey ValidationKey { get; }

    /// <summary>Ready-to-use signing credentials for the token descriptor.</summary>
    public SigningCredentials SigningCredentials { get; }

    [LoggerMessage(
        EventId = 3100,
        Level = LogLevel.Warning,
        Message = "No JWT signing key configured; generated an ephemeral key. Tokens will not survive a restart. Configure Jwt:PrivateKeyPem for any non-local environment.")]
    private static partial void EphemeralKeyGenerated(ILogger logger);

    public void Dispose()
    {
        _rsa?.Dispose();
        _ecdsa?.Dispose();
    }
}
