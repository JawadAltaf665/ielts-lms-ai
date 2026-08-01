using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Speaking;

internal sealed class SpeakingEvaluationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<SpeakingEvaluation, SpeakingEvaluationId>(dbContext), ISpeakingEvaluationRepository
{
    public async Task<SpeakingEvaluation?> GetByAttemptIdAsync(SpeakingAttemptId attemptId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(e => e.SpeakingAttemptId == attemptId, cancellationToken).ConfigureAwait(false);
}
