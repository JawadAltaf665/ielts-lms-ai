namespace IELTSLms.Application.Identity;

/// <summary>
/// The token pair returned by login and refresh. The refresh token is opaque and must be delivered
/// to the client over a secure channel (the API sets it as an HttpOnly cookie); it is never logged
/// or stored in plaintext server-side (Auth §3.1, §4).
/// </summary>
/// <param name="AccessToken">The signed JWT access token.</param>
/// <param name="AccessTokenExpiresAtUtc">When the access token expires (UTC).</param>
/// <param name="RefreshToken">The opaque refresh token (raw value; hashed at rest).</param>
/// <param name="RefreshTokenExpiresAtUtc">When the refresh token / session window expires (UTC).</param>
/// <param name="SessionId">The device session id (surfaced as the <c>sid</c> claim).</param>
public sealed record AuthTokensResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid SessionId);
