using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;

public sealed class CreateSpeakingTaskCommandHandler(
    ISpeakingTaskRepository tasks, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateSpeakingTaskCommand, SpeakingTaskResponse>
{
    public async Task<Result<SpeakingTaskResponse>> Handle(CreateSpeakingTaskCommand request, CancellationToken cancellationToken)
    {
        // Tasks created through the tenant-facing API are always tenant-owned; global-catalog tasks
        // (TenantId null) are seeded content, not authored here.
        if (await tasks.TitleExistsAsync(request.TenantId, request.Title, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<SpeakingTaskResponse>(SpeakingErrors.TaskTitleTaken);
        }

        var now = clock.UtcNow;
        var createResult = SpeakingTask.Create(
            SpeakingTaskId.From(guids.NewGuid()), request.TenantId, request.Title, request.Part, request.PromptText,
            request.CategoryId, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<SpeakingTaskResponse>(createResult.Error);
        }

        var task = createResult.Value;
        tasks.Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(task));
    }

    internal static SpeakingTaskResponse ToResponse(SpeakingTask task) => new(
        task.Id.Value, task.TenantId, task.Title, task.Part.ToString(), task.PromptText, task.CategoryId, task.IsGlobal);
}
