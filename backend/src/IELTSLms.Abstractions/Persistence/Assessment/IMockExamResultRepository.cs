using IELTSLms.Domain.Assessment;

namespace IELTSLms.Abstractions.Persistence.Assessment;

/// <summary>Aggregate access for <see cref="MockExamResult"/> (ERD ENT-037), 1:1 with its <see cref="MonthlyMockExam"/>.</summary>
public interface IMockExamResultRepository : IRepository<MockExamResult, MockExamResultId>
{
    Task<MockExamResult?> GetByMockExamIdAsync(MockExamId mockExamId, CancellationToken cancellationToken = default);

    /// <summary>A user's finalized results, newest first — feeds the Sprint 9 cross-skill exam history
    /// and Band Prediction's history gathering.</summary>
    Task<IReadOnlyList<MockExamResult>> ListByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>The most recently finalized result for a user, or <see langword="null"/> if none —
    /// used by <c>CertificateIssuanceService</c> to (re-)evaluate eligibility off <c>ProgressRecalculated</c>.</summary>
    Task<MockExamResult?> GetLatestFinalizedByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
