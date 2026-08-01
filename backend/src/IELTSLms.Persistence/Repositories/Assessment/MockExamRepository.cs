using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Domain.Assessment;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Assessment;

internal sealed class MockExamRepository(ApplicationDbContext dbContext)
    : RepositoryBase<MonthlyMockExam, MockExamId>(dbContext), IMockExamRepository
{
    public async Task<MonthlyMockExam?> GetActiveInProgressByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .FirstOrDefaultAsync(
                e => e.TenantId == tenantId && e.UserId == userId && e.State == MockExamState.InProgress, cancellationToken)
            .ConfigureAwait(false);
}
