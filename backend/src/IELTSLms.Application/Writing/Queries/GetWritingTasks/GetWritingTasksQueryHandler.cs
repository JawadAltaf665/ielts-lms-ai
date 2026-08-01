using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Queries.GetWritingTasks;

public sealed class GetWritingTasksQueryHandler(IWritingTaskRepository tasks)
    : IQueryHandler<GetWritingTasksQuery, PagedList<WritingTaskResponse>>
{
    public async Task<Result<PagedList<WritingTaskResponse>>> Handle(GetWritingTasksQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await tasks.ListVisibleAsync(
            request.TenantId, request.Track, request.TaskType, skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var total = await tasks.CountVisibleAsync(request.TenantId, request.Track, request.TaskType, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<WritingTaskResponse>.Create(
            items.Select(CreateWritingTaskCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
