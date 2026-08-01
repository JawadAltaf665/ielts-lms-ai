namespace IELTSLms.Domain.Assessment;

/// <summary>
/// Encapsulates <see cref="MonthlyMockExam"/> creation (DDD §7 "encapsulates complex creation with
/// invariants"): a valid mock exam is born with all four section slots defined (present but
/// not-yet-started) rather than assembled incrementally by the caller.
/// </summary>
public static class MockExamFactory
{
    public static MonthlyMockExam Start(MockExamId id, Guid tenantId, Guid userId, DateTimeOffset nowUtc) =>
        MonthlyMockExam.Start(id, tenantId, userId, nowUtc);
}
