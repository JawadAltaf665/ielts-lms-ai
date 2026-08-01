using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Commands.CreateWritingTask;

public sealed class CreateWritingTaskCommandHandler(
    IWritingTaskRepository tasks, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateWritingTaskCommand, WritingTaskResponse>
{
    public async Task<Result<WritingTaskResponse>> Handle(CreateWritingTaskCommand request, CancellationToken cancellationToken)
    {
        // Tasks created through the tenant-facing API are always tenant-owned; global-catalog tasks
        // (TenantId null) are seeded content, not authored here.
        if (await tasks.TitleExistsAsync(request.TenantId, request.Title, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<WritingTaskResponse>(WritingErrors.TaskTitleTaken);
        }

        var now = clock.UtcNow;
        var createResult = WritingTask.Create(
            WritingTaskId.From(guids.NewGuid()), request.TenantId, request.Title, request.TaskType, request.Track, request.PromptText,
            request.RubricReference, request.MinWordCount, request.MaxWordCount, request.CategoryId, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<WritingTaskResponse>(createResult.Error);
        }

        var task = createResult.Value;
        tasks.Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(task));
    }

    internal static WritingTaskResponse ToResponse(WritingTask task) => new(
        task.Id.Value, task.TenantId, task.Title, task.TaskType.ToString(), task.Track.ToString(), task.PromptText,
        task.RubricReference, task.MinWordCount, task.MaxWordCount, task.CategoryId, task.IsGlobal);
}
