using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.GetGrammarTopics;

public sealed class GetGrammarTopicsQueryHandler(IGrammarTopicRepository topics)
    : IQueryHandler<GetGrammarTopicsQuery, PagedList<GrammarTopicResponse>>
{
    public async Task<Result<PagedList<GrammarTopicResponse>>> Handle(GetGrammarTopicsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await topics.ListVisibleAsync(
            request.TenantId, request.Track, request.Difficulty, skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var total = await topics.CountVisibleAsync(request.TenantId, request.Track, request.Difficulty, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<GrammarTopicResponse>.Create(
            items.Select(CreateGrammarTopicCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
