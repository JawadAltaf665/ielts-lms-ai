using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.GetVocabularyWords;

public sealed class GetVocabularyWordsQueryHandler(IVocabularyWordRepository words)
    : IQueryHandler<GetVocabularyWordsQuery, PagedList<VocabularyWordResponse>>
{
    public async Task<Result<PagedList<VocabularyWordResponse>>> Handle(GetVocabularyWordsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await words.ListVisibleAsync(
            request.TenantId, request.Topic, request.DifficultyLevel, request.Search, skip, request.PageSize, cancellationToken)
            .ConfigureAwait(false);
        var total = await words.CountVisibleAsync(request.TenantId, request.Topic, request.DifficultyLevel, request.Search, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<VocabularyWordResponse>.Create(
            items.Select(CreateVocabularyWordCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
