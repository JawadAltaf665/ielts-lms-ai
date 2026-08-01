using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Vocabulary.Events;

/// <summary>
/// Raised when a learner grades one vocabulary review (DDD §9.2). Handled by a post-commit domain-event
/// handler that reports the activity to the shared, context-agnostic <c>IProgressTrackingService</c> —
/// the same pattern <see cref="Grammar.Events.GrammarCompletedDomainEvent"/> established in Sprint 1.
/// Unlike Grammar, there is no separate per-topic rollup entity to update here: <see cref="VocabularyProgress"/>
/// itself already is that per-word progress state, updated synchronously by the grading command.
/// </summary>
public sealed record VocabularyCompletedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid VocabularyWordId,
    int Grade) : DomainEvent;
