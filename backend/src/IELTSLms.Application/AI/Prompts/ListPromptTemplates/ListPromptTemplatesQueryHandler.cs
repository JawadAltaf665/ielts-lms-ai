using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Prompts.ListPromptTemplates;

public sealed class ListPromptTemplatesQueryHandler(IAiPromptRepository prompts)
    : IQueryHandler<ListPromptTemplatesQuery, PagedList<AiPromptResponse>>
{
    public async Task<Result<PagedList<AiPromptResponse>>> Handle(ListPromptTemplatesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await prompts
            .ListAsync(request.TenantId, request.Feature, request.Scope, request.IsActive, skip, request.PageSize, cancellationToken)
            .ConfigureAwait(false);
        var total = await prompts
            .CountAsync(request.TenantId, request.Feature, request.Scope, request.IsActive, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<AiPromptResponse>.Create(
            items.Select(AiPromptResponseMapper.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
