using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarExerciseRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarExercise, GrammarExerciseId>(dbContext), IGrammarExerciseRepository
{
    public async Task<GrammarExercise?> GetVisibleByIdAsync(Guid tenantId, GrammarExerciseId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id && (e.TenantId == null || e.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<GrammarExercise>> ListByTopicAsync(
        Guid tenantId, GrammarTopicId topicId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(e => e.GrammarTopicId == topicId && (e.TenantId == null || e.TenantId == tenantId))
            .OrderBy(e => e.Order)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
