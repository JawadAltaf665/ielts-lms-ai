using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Domain.Listening;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Listening;

internal sealed class ListeningQuestionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<ListeningQuestion, ListeningQuestionId>(dbContext), IListeningQuestionRepository
{
    public async Task<IReadOnlyList<ListeningQuestion>> ListByAudioAsync(
        ListeningAudioId audioId, CancellationToken cancellationToken = default) =>
        await Set.Where(q => q.ListeningAudioId == audioId)
            .OrderBy(q => q.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> SequenceExistsAsync(
        ListeningAudioId audioId, int sequence, ListeningQuestionId? excludingId, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(
            q => q.ListeningAudioId == audioId && q.Sequence == sequence && (!excludingId.HasValue || q.Id != excludingId.Value),
            cancellationToken).ConfigureAwait(false);
}
