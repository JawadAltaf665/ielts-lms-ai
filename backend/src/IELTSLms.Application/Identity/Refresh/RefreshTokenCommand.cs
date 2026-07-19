using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Refresh;

/// <summary>
/// Exchanges a valid refresh token for a new token pair, rotating the old one (Auth §4.2). Presenting
/// an already-used token is treated as a breach and revokes the whole session family (Auth §4.3).
/// </summary>
/// <param name="RefreshToken">The opaque refresh token presented by the client.</param>
/// <param name="IpAddress">Caller IP, for session metadata (nullable).</param>
/// <param name="UserAgent">Caller user-agent, for session metadata (nullable).</param>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress,
    string? UserAgent) : ICommand<AuthTokensResult>;
