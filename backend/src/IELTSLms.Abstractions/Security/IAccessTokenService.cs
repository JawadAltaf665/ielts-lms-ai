namespace IELTSLms.Abstractions.Security;

/// <summary>
/// Issues signed JWT access tokens from a minimal, tenant-scoped claim set (Auth §3.2, §9.1).
/// Signing algorithm, keys, issuer, audience, and lifetime are configuration-driven. Validation of
/// incoming tokens is handled by the ASP.NET Core JWT bearer middleware at the API boundary, not here.
/// </summary>
public interface IAccessTokenService
{
    /// <summary>Creates a signed access token for the given identity and its expiry instant.</summary>
    AccessToken CreateToken(AccessTokenRequest request);
}

/// <summary>The identity a token is minted for. Kept as primitives so this port stays domain-free.</summary>
/// <param name="UserId">Subject (<c>sub</c>) — the stable user identifier.</param>
/// <param name="TenantId">Tenant scope (<c>tenantId</c>) validated on every request.</param>
/// <param name="SessionId">Device session (<c>sid</c>) linking the token to a session.</param>
/// <param name="Email">The user's email, included for convenience (non-sensitive display claim).</param>
/// <param name="AuthenticationMethods">Auth methods (<c>amr</c>), e.g. <c>pwd</c> (MFA-ready).</param>
public readonly record struct AccessTokenRequest(
    Guid UserId,
    Guid TenantId,
    Guid SessionId,
    string Email,
    IReadOnlyList<string> AuthenticationMethods);

/// <summary>A minted access token and the instant it expires.</summary>
/// <param name="Value">The compact-serialized, signed JWT.</param>
/// <param name="ExpiresAtUtc">When the token expires (UTC), for the client to schedule refresh.</param>
public readonly record struct AccessToken(string Value, DateTimeOffset ExpiresAtUtc);
