using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Learning.Events;

/// <summary>Raised when a <see cref="DailyPlan"/> is generated (DDD §13.4 event catalog). Consumed by
/// Notifications to alert the learner their tasks are ready — that delivery is explicitly out of this
/// sprint's scope (Sprint 9 scope notes); this event is the hand-off point for it.</summary>
public sealed record DailyPlanGeneratedDomainEvent(Guid TenantId, Guid UserId, Guid DailyPlanId, DateOnly PlanDate) : DomainEvent;
