using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.SentenceBuilder;

internal sealed class SentenceExerciseRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SentenceExercise, SentenceExerciseId>(dbContext), ISentenceExerciseRepository
{
    public async Task<SentenceExercise?> GetVisibleByIdAsync(
        Guid tenantId, SentenceExerciseId id, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id && (e.TenantId == null || e.TenantId == tenantId), cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<SentenceExercise>> ListVisibleAsync(
        Guid tenantId, SentenceDifficulty? difficulty, int skip, int take, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, difficulty)
            .OrderBy(e => e.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<long> CountVisibleAsync(
        Guid tenantId, SentenceDifficulty? difficulty, CancellationToken cancellationToken = default) =>
        await Visible(tenantId, difficulty).LongCountAsync(cancellationToken).ConfigureAwait(false);

    private IQueryable<SentenceExercise> Visible(Guid tenantId, SentenceDifficulty? difficulty)
    {
        var query = Set.Where(e => e.TenantId == null || e.TenantId == tenantId);

        if (difficulty is { } difficultyValue)
        {
            query = query.Where(e => e.Difficulty == difficultyValue);
        }

        return query;
    }
}
