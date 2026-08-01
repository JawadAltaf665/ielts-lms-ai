using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Domain.Assessment;

namespace IELTSLms.Application.Tests.Assessment.Fakes;

internal sealed class FakeMockExamResultRepository : IMockExamResultRepository
{
    private readonly List<MockExamResult> _results = [];

    public void Seed(MockExamResult result) => _results.Add(result);

    public IReadOnlyList<MockExamResult> Results => _results;

    public Task<MockExamResult?> GetByIdAsync(MockExamResultId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_results.FirstOrDefault(r => r.Id == id));

    public Task<MockExamResult?> GetByMockExamIdAsync(MockExamId mockExamId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_results.FirstOrDefault(r => r.MockExamId == mockExamId));

    public void Add(MockExamResult aggregate) => _results.Add(aggregate);

    public void Remove(MockExamResult aggregate) => _results.Remove(aggregate);

    public Task<IReadOnlyList<MockExamResult>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MockExamResult>>(_results
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsFinalized)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToList());

    public Task<MockExamResult?> GetLatestFinalizedByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_results
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsFinalized)
            .OrderByDescending(r => r.FinalizedAtUtc)
            .FirstOrDefault());
}
