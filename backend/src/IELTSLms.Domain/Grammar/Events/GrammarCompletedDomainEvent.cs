using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Grammar.Events;

/// <summary>
/// Raised when a learner's <see cref="GrammarAttempt"/> is scored (DDD §9.2). Drives two reactions
/// via a post-commit domain-event handler: Grammar's own per-topic <see cref="GrammarProgress"/>, and
/// the cross-module canonical <c>ProgressTracking</c> (ERD ENT-055) through
/// <c>IProgressTrackingService</c> — the pattern every later skill module (Vocabulary, Reading, ...)
/// repeats for its own <c>&lt;Skill&gt;Completed</c> event.
/// </summary>
public sealed record GrammarCompletedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid GrammarTopicId,
    Guid GrammarExerciseId,
    Guid GrammarAttemptId,
    int RawScore,
    int MaxScore) : DomainEvent;
