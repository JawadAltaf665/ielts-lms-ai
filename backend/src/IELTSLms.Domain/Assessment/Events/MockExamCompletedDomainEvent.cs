using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Assessment.Events;

/// <summary>
/// Raised when all four sections of a <see cref="MonthlyMockExam"/> are evaluated and its
/// <see cref="MockExamResult"/> is finalized (DDD §9.2, EVT-ASM-01).
/// </summary>
public sealed record MockExamCompletedDomainEvent(
    Guid TenantId,
    Guid UserId,
    Guid MockExamId,
    decimal OverallBand) : DomainEvent;
