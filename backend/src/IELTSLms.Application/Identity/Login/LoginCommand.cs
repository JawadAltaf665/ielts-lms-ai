using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Login;

/// <summary>
/// Authenticates with email + password and, on success, opens a device session and issues a token
/// pair (Auth §3, §4). Client metadata is captured for the session record and login history.
/// </summary>
/// <param name="Email">The account email.</param>
/// <param name="Password">The plaintext password.</param>
/// <param name="IpAddress">Caller IP, for the session and audit trail (nullable).</param>
/// <param name="UserAgent">Caller user-agent, for the session and audit trail (nullable).</param>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent) : ICommand<AuthTokensResult>;
