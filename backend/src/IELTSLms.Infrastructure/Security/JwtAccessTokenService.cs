using IELTSLms.Abstractions.Security;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Mints signed JWT access tokens with a minimal, tenant-scoped claim set (Auth §3.2, §9.1). Signing
/// is asymmetric (RS256/ES256) via the shared <see cref="JwtSigningKeyProvider"/>, so the same key is
/// used to validate tokens on the API. Incoming-token validation lives in the API's JWT bearer
/// middleware, not here.
/// </summary>
public sealed class JwtAccessTokenService(
    JwtSigningKeyProvider keyProvider,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthOptions> authOptions,
    IClock clock)
    : IAccessTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly AuthOptions _authOptions = authOptions.Value;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    /// <inheritdoc />
    public AccessToken CreateToken(AccessTokenRequest request)
    {
        var issuedAt = clock.UtcNow;
        var expiresAt = issuedAt.Add(_authOptions.AccessTokenLifetime);

        // Minimal, tenant-scoped claims only — no PII beyond the display email (Auth §9.1).
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = request.UserId.ToString(),
            ["tenantId"] = request.TenantId.ToString(),
            ["sid"] = request.SessionId.ToString(),
            [JwtRegisteredClaimNames.Email] = request.Email,
            ["amr"] = request.AuthenticationMethods,
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = claims,
            SigningCredentials = keyProvider.SigningCredentials,
        };

        var token = _tokenHandler.CreateToken(descriptor);
        return new AccessToken(token, expiresAt);
    }
}
