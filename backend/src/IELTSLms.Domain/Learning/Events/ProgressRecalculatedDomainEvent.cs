using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Learning.Events;

/// <summary>Raised whenever a <see cref="ProgressTracking"/> row is updated (DDD §13.15 BC-15,
/// POL-06: "whenever criteria are met (ProgressRecalculated), then evaluate certificate eligibility").</summary>
public sealed record ProgressRecalculatedDomainEvent(Guid TenantId, Guid UserId, LearningSkill Skill) : DomainEvent;
