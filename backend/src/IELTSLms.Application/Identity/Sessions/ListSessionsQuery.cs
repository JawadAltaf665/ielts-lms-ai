using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Identity.Sessions;

/// <summary>Lists the caller's active device sessions so they can review and revoke them (Auth §4).</summary>
/// <param name="UserId">The authenticated user, from the access-token subject.</param>
public sealed record ListSessionsQuery(Guid UserId) : IQuery<IReadOnlyList<SessionSummary>>;

/// <summary>A single active session, safe to expose to its owner.</summary>
/// <param name="SessionId">The session identifier (used to target revocation).</param>
/// <param name="DeviceLabel">Best-effort device description, if captured.</param>
/// <param name="IpAddress">IP the session was last seen from, if captured.</param>
/// <param name="LastSeenAtUtc">When the session was last active (UTC).</param>
/// <param name="CreatedAtUtc">When the session started (UTC).</param>
/// <param name="ExpiresAtUtc">When the session expires (UTC).</param>
public sealed record SessionSummary(
    Guid SessionId,
    string? DeviceLabel,
    string? IpAddress,
    DateTimeOffset LastSeenAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc);
