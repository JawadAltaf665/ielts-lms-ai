using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.GetVocabularyWord;

public sealed class GetVocabularyWordQueryHandler(IVocabularyWordRepository words)
    : IQueryHandler<GetVocabularyWordQuery, VocabularyWordResponse>
{
    public async Task<Result<VocabularyWordResponse>> Handle(GetVocabularyWordQuery request, CancellationToken cancellationToken)
    {
        var word = await words.GetVisibleByIdAsync(request.TenantId, VocabularyWordId.From(request.WordId), cancellationToken)
            .ConfigureAwait(false);
        if (word is null)
        {
            return Result.Failure<VocabularyWordResponse>(VocabularyErrors.WordNotFound);
        }

        return Result.Success(CreateVocabularyWordCommandHandler.ToResponse(word));
    }
}
