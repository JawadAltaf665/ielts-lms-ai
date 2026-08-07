using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.AI.Events;

/// <summary>
/// Raised exactly once when a <see cref="LessonSession"/>'s step sequence reaches
/// <see cref="LessonStep.Completed"/> (Phase 7 Sprint 4). Drives the same cross-module
/// <c>ProgressTracking</c> reaction <see cref="Grammar.Events.GrammarCompletedDomainEvent"/>
/// established in Phase 6 Sprint 1 — no parallel progress-update path.
/// </summary>
public sealed record LessonSessionCompletedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid LessonId,
    Guid LessonSessionId) : DomainEvent;
