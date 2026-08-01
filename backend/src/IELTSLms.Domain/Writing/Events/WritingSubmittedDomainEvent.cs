using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Writing.Events;

/// <summary>
/// Raised when a <see cref="WritingSubmission"/> is submitted (DDD §9.2, EVT-WRT-01). No handler
/// subscribes to this in Sprint 6 — the AI-evaluation kickoff this event would otherwise trigger
/// (POL-03) is instead performed directly inside <c>SubmitWritingCommandHandler</c>, in the same
/// transaction that creates the <c>AiOperation</c>, so the 202 response can return that operation's id
/// without a reentrant nested <c>SaveChangesAsync</c> from within post-commit event dispatch. Still
/// raised for audit/future-subscriber parity with the DDD event catalog.
/// </summary>
public sealed record WritingSubmittedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid WritingTaskId,
    Guid WritingSubmissionId) : DomainEvent;
