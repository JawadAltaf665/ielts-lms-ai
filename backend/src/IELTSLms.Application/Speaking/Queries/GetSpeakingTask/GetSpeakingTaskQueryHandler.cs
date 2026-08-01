using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingTask;

public sealed class GetSpeakingTaskQueryHandler(ISpeakingTaskRepository tasks) : IQueryHandler<GetSpeakingTaskQuery, SpeakingTaskResponse>
{
    public async Task<Result<SpeakingTaskResponse>> Handle(GetSpeakingTaskQuery request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetVisibleByIdAsync(request.TenantId, SpeakingTaskId.From(request.TaskId), cancellationToken)
            .ConfigureAwait(false);
        return task is null
            ? Result.Failure<SpeakingTaskResponse>(SpeakingErrors.TaskNotFound)
            : Result.Success(CreateSpeakingTaskCommandHandler.ToResponse(task));
    }
}
