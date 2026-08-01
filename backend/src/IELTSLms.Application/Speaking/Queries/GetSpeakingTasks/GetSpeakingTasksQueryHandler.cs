using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingTasks;

public sealed class GetSpeakingTasksQueryHandler(ISpeakingTaskRepository tasks)
    : IQueryHandler<GetSpeakingTasksQuery, PagedList<SpeakingTaskResponse>>
{
    public async Task<Result<PagedList<SpeakingTaskResponse>>> Handle(GetSpeakingTasksQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await tasks.ListVisibleAsync(request.TenantId, request.Part, skip, request.PageSize, cancellationToken)
            .ConfigureAwait(false);
        var total = await tasks.CountVisibleAsync(request.TenantId, request.Part, cancellationToken).ConfigureAwait(false);

        var page = PagedList<SpeakingTaskResponse>.Create(
            items.Select(CreateSpeakingTaskCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
