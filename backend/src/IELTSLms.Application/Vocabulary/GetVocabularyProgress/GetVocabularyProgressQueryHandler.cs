using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.GradeVocabularyReview;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.GetVocabularyProgress;

public sealed class GetVocabularyProgressQueryHandler(IVocabularyProgressRepository progress)
    : IQueryHandler<GetVocabularyProgressQuery, IReadOnlyList<VocabularyProgressResponse>>
{
    private static readonly Error NotFound = Error.NotFound(
        "NOT_FOUND_VOCABULARY_PROGRESS", "Vocabulary progress not found.");

    public async Task<Result<IReadOnlyList<VocabularyProgressResponse>>> Handle(
        GetVocabularyProgressQuery request, CancellationToken cancellationToken)
    {
        if (request.CallerUserId != request.RequestedUserId)
        {
            return Result.Failure<IReadOnlyList<VocabularyProgressResponse>>(NotFound);
        }

        var rows = await progress.ListByUserAsync(request.TenantId, request.RequestedUserId, cancellationToken).ConfigureAwait(false);

        IReadOnlyList<VocabularyProgressResponse> response = rows.Select(GradeVocabularyReviewCommandHandler.ToResponse).ToArray();

        return Result.Success(response);
    }
}
