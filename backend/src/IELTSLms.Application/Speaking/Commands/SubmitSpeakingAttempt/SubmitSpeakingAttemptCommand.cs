using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Speaking.Commands.SubmitSpeakingAttempt;

/// <summary>
/// Marks a recorded <see cref="Domain.Speaking.SpeakingAttempt"/> submitted and kicks off the async
/// transcription + evaluation workflow directly (entitlement/quota check, <c>AiOperation</c> creation,
/// job enqueue) rather than via a domain-event handler — see
/// <see cref="Domain.Speaking.Events.SpeakingSubmittedDomainEvent"/>'s remarks for why. Mirrors
/// <c>SubmitWritingCommandHandler</c>'s AI-kickoff shape. Returns <c>202 Accepted</c> plus an operation
/// resource; no request body beyond the attempt id (the recording was already uploaded via
/// <c>CreateSpeakingAttemptCommand</c>).
/// </summary>
public sealed record SubmitSpeakingAttemptCommand(Guid TenantId, Guid UserId, Guid AttemptId)
    : ICommand<SpeakingAttemptAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.submit"];
}

public sealed record SpeakingAttemptAcceptedResponse(Guid AttemptId, Guid OperationId, string Status);
