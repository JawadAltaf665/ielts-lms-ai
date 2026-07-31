using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Application.Listening.StartListeningAttempt;

/// <summary>
/// Starts a timed attempt: stamps <c>StartedAt</c> and computes <c>DeadlineAt</c> on the server from
/// the audio's server-configured duration, and stamps <see cref="Mode"/> for the life of the attempt
/// (governs the replay cap and transcript visibility) — same server-owns-timing rule as
/// <c>StartReadingAttemptCommand</c> (Phase 6 Sprint 4).
/// </summary>
public sealed record StartListeningAttemptCommand(Guid TenantId, Guid UserId, Guid AudioId, ListeningMode Mode)
    : ICommand<ListeningAttemptStartResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.attempt"];
}

/// <summary><see cref="AllowedReplays"/> is <c>-1</c> for unlimited (learning mode).</summary>
public sealed record ListeningAttemptStartResponse(
    Guid Id, Guid AudioId, string Mode, DateTimeOffset StartedAtUtc, DateTimeOffset DeadlineAtUtc, int AllowedReplays);
