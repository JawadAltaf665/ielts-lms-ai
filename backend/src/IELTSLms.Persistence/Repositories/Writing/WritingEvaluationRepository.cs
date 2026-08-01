using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Writing;

internal sealed class WritingEvaluationRepository(ApplicationDbContext dbContext)
    : RepositoryBase<WritingEvaluation, WritingEvaluationId>(dbContext), IWritingEvaluationRepository
{
    public async Task<WritingEvaluation?> GetBySubmissionIdAsync(WritingSubmissionId submissionId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(e => e.WritingSubmissionId == submissionId, cancellationToken).ConfigureAwait(false);
}
