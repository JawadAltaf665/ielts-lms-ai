using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.SentenceBuilder.Events;

/// <summary>
/// Raised when a <see cref="SentenceAttempt"/> is scored (DDD §9.2-style event, Sprint 7). Unlike
/// <c>WritingSubmittedDomainEvent</c> (which bypasses its own event for the AI kickoff so a 202 response
/// can return the freshly-created operation's id — see that event's remarks), this endpoint's response
/// is just the synchronous deterministic score, so there is no reentrancy concern: the post-commit
/// handler enqueueing the AI scaffolding job here is safe.
/// </summary>
public sealed record SentenceAttemptSubmittedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid SentenceExerciseId,
    Guid SentenceAttemptId,
    string SubmittedText) : DomainEvent;
