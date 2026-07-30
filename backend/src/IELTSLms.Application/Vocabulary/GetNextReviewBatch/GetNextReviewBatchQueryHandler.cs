using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Application.Vocabulary.GradeVocabularyReview;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Vocabulary.GetNextReviewBatch;

public sealed class GetNextReviewBatchQueryHandler(
    IVocabularyWordRepository words, IVocabularyProgressRepository progressRepository, IOptions<VocabularyOptions> options, IClock clock)
    : IQueryHandler<GetNextReviewBatchQuery, IReadOnlyList<VocabularyReviewItemResponse>>
{
    public async Task<Result<IReadOnlyList<VocabularyReviewItemResponse>>> Handle(
        GetNextReviewBatchQuery request, CancellationToken cancellationToken)
    {
        var batchSize = ClampBatchSize(request.BatchSize, options.Value);
        var now = clock.UtcNow;

        var dueProgress = await progressRepository.ListDueAsync(request.TenantId, request.UserId, now, batchSize, cancellationToken)
            .ConfigureAwait(false);
        var dueWords = await words.GetByIdsAsync(dueProgress.Select(p => p.VocabularyWordId).ToArray(), cancellationToken)
            .ConfigureAwait(false);
        var dueWordsById = dueWords.ToDictionary(w => w.Id);

        var items = new List<VocabularyReviewItemResponse>(batchSize);
        foreach (var progress in dueProgress)
        {
            if (dueWordsById.TryGetValue(progress.VocabularyWordId, out var word))
            {
                items.Add(new VocabularyReviewItemResponse(
                    CreateVocabularyWordCommandHandler.ToResponse(word), GradeVocabularyReviewCommandHandler.ToResponse(progress)));
            }
        }

        var remaining = batchSize - items.Count;
        if (remaining > 0)
        {
            var newWords = await words.ListNewForUserAsync(request.TenantId, request.UserId, remaining, cancellationToken)
                .ConfigureAwait(false);
            items.AddRange(newWords.Select(w => new VocabularyReviewItemResponse(CreateVocabularyWordCommandHandler.ToResponse(w), null)));
        }

        return Result.Success<IReadOnlyList<VocabularyReviewItemResponse>>(items);
    }

    private static int ClampBatchSize(int? requested, VocabularyOptions options) =>
        requested is { } value ? Math.Clamp(value, 1, options.MaxReviewBatchSize) : options.DefaultReviewBatchSize;
}
