using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Grammar;

internal sealed class GrammarQuestionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<GrammarQuestion, GrammarQuestionId>(dbContext), IGrammarQuestionRepository
{
    public async Task<IReadOnlyList<GrammarQuestion>> ListByExerciseAsync(
        GrammarExerciseId exerciseId, CancellationToken cancellationToken = default) =>
        await Set
            .Where(q => q.GrammarExerciseId == exerciseId)
            .OrderBy(q => q.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> SequenceExistsAsync(
        GrammarExerciseId exerciseId, int sequence, GrammarQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            q => q.GrammarExerciseId == exerciseId && q.Sequence == sequence && (!excludingId.HasValue || q.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);
}
