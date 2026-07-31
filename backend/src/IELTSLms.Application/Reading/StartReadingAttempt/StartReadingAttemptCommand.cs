using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Reading.StartReadingAttempt;

/// <summary>
/// Starts a timed attempt: stamps <c>StartedAt</c> and computes <c>DeadlineAt</c> on the server from
/// the passage's server-configured duration — the caller never supplies timing (Sprint 4 "Business
/// rules — timer enforcement").
/// </summary>
public sealed record StartReadingAttemptCommand(Guid TenantId, Guid UserId, Guid PassageId)
    : ICommand<ReadingAttemptStartResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.attempt"];
}

public sealed record ReadingAttemptStartResponse(Guid Id, Guid PassageId, DateTimeOffset StartedAtUtc, DateTimeOffset DeadlineAtUtc);
