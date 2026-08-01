using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Queries.GetWritingTask;

public sealed class GetWritingTaskQueryHandler(IWritingTaskRepository tasks) : IQueryHandler<GetWritingTaskQuery, WritingTaskResponse>
{
    public async Task<Result<WritingTaskResponse>> Handle(GetWritingTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetVisibleByIdAsync(request.TenantId, WritingTaskId.From(request.TaskId), cancellationToken)
            .ConfigureAwait(false);
        return task is null
            ? Result.Failure<WritingTaskResponse>(WritingErrors.TaskNotFound)
            : Result.Success(CreateWritingTaskCommandHandler.ToResponse(task));
    }
}
