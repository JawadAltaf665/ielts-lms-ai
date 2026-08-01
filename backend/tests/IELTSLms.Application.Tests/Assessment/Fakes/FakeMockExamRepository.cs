using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Domain.Assessment;

namespace IELTSLms.Application.Tests.Assessment.Fakes;

/// <summary>
/// Backed by a plain list that mutates immediately on <see cref="Add"/> (like every other fake
/// repository in this codebase) rather than deferring until <c>SaveChangesAsync</c> — calling a
/// handler twice against the same instance is how the "two simultaneous StartMockExam calls" guard is
/// exercised deterministically (see <c>StartMockExamCommandHandlerTests</c>).
/// </summary>
internal sealed class FakeMockExamRepository : IMockExamRepository
{
    private readonly List<MonthlyMockExam> _exams = [];

    public void Seed(MonthlyMockExam exam) => _exams.Add(exam);

    public IReadOnlyList<MonthlyMockExam> Exams => _exams;

    public Task<MonthlyMockExam?> GetByIdAsync(MockExamId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exams.FirstOrDefault(e => e.Id == id));

    public Task<MonthlyMockExam?> GetActiveInProgressByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_exams.FirstOrDefault(e => e.TenantId == tenantId && e.UserId == userId && e.State == MockExamState.InProgress));

    public void Add(MonthlyMockExam aggregate) => _exams.Add(aggregate);

    public void Remove(MonthlyMockExam aggregate) => _exams.Remove(aggregate);
}
