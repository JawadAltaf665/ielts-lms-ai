using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Domain.Assessment;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Assessment;

internal sealed class MockExamResultRepository(ApplicationDbContext dbContext)
    : RepositoryBase<MockExamResult, MockExamResultId>(dbContext), IMockExamResultRepository
{
    public async Task<MockExamResult?> GetByMockExamIdAsync(MockExamId mockExamId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(r => r.MockExamId == mockExamId, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<MockExamResult>> ListByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsFinalized)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<MockExamResult?> GetLatestFinalizedByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(r => r.TenantId == tenantId && r.UserId == userId && r.IsFinalized)
            .OrderByDescending(r => r.FinalizedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
}
