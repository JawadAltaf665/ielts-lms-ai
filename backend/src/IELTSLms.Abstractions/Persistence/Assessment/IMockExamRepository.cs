using IELTSLms.Domain.Assessment;

namespace IELTSLms.Abstractions.Persistence.Assessment;

/// <summary>Aggregate access for <see cref="MonthlyMockExam"/> (ERD ENT-036).</summary>
public interface IMockExamRepository : IRepository<MonthlyMockExam, MockExamId>
{
    /// <summary>The caller's currently <see cref="MockExamState.InProgress"/> session, if any — backs the single-active-session invariant and the event-driven section-completion lookup.</summary>
    Task<MonthlyMockExam?> GetActiveInProgressByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
