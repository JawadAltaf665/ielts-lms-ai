using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Reading;

internal sealed class ReadingQuestionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ReadingQuestion, ReadingQuestionId>(dbContext), IReadingQuestionRepository
{
    public async Task<IReadOnlyList<ReadingQuestion>> ListByPassageAsync(
        ReadingPassageId passageId, CancellationToken cancellationToken = default) =>
        await Set.Where(q => q.ReadingPassageId == passageId)
            .OrderBy(q => q.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> SequenceExistsAsync(
        ReadingPassageId passageId, int sequence, ReadingQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            q => q.ReadingPassageId == passageId && q.Sequence == sequence && (!excludingId.HasValue || q.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);
}
