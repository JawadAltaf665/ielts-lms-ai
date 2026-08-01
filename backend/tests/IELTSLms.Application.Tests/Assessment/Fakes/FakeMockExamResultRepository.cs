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
}
