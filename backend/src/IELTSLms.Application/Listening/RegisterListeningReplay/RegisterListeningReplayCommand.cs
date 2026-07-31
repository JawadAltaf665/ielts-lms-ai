using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Listening.RegisterListeningReplay;

/// <summary>
/// Registers one replay against the attempt's server-tracked cap and, on success, issues a fresh
/// signed playback URL for it (Sprint 5 business rule: "every replay goes through this endpoint before
/// the client is served a fresh signed stream URL for that replay"). <c>409 BUSINESS_REPLAY_LIMIT_EXCEEDED</c>
/// once the mode's cap is reached — never trust a client-reported replay count.
/// </summary>
public sealed record RegisterListeningReplayCommand(Guid TenantId, Guid UserId, Guid AttemptId)
    : ICommand<ListeningReplayResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.attempt"];
}

public sealed record ListeningReplayResponse(int ReplayCount, string Url, DateTimeOffset ExpiresAtUtc);
