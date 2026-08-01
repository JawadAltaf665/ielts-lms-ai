using IELTSLms.Domain.Assessment;

namespace IELTSLms.Abstractions.Persistence.Assessment;

/// <summary>Aggregate access for <see cref="MockExamResult"/> (ERD ENT-037), 1:1 with its <see cref="MonthlyMockExam"/>.</summary>
public interface IMockExamResultRepository : IRepository<MockExamResult, MockExamResultId>
{
    Task<MockExamResult?> GetByMockExamIdAsync(MockExamId mockExamId, CancellationToken cancellationToken = default);
}
