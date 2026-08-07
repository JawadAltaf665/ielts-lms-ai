using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.AI;

internal sealed class LessonSessionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<LessonSession, LessonSessionId>(dbContext), ILessonSessionRepository
{
    public async Task<LessonSession?> GetActiveByUserAndLessonAsync(
        Guid tenantId, Guid userId, Guid lessonId, CancellationToken cancellationToken = default) =>
        await Set
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId && s.UserId == userId && s.LessonId == lessonId && s.CurrentStep != LessonStep.Completed,
                cancellationToken)
            .ConfigureAwait(false);
}
