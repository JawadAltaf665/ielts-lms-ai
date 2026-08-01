using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Domain.Assessment;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Assessment;

internal sealed class MockExamResultRepository(ApplicationDbContext dbContext)
    : RepositoryBase<MockExamResult, MockExamResultId>(dbContext), IMockExamResultRepository
{
    public async Task<MockExamResult?> GetByMockExamIdAsync(MockExamId mockExamId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(r => r.MockExamId == mockExamId, cancellationToken).ConfigureAwait(false);
}
