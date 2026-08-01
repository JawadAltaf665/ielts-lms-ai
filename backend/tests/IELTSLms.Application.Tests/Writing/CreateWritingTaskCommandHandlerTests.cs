using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class CreateWritingTaskCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_task()
    {
        var tasks = new FakeWritingTaskRepository();
        var handler = new CreateWritingTaskCommandHandler(tasks, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateWritingTaskCommand(
                tenantId, "Task 2 - Opinion Essay", WritingTaskType.Task2, ReadingTrack.Academic, "Discuss...", null, 250, 400, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Single(tasks.Tasks);
    }

    [Fact]
    public async Task Duplicate_title_within_the_same_tenant_is_rejected()
    {
        var tasks = new FakeWritingTaskRepository();
        var tenantId = Guid.NewGuid();
        tasks.Seed(WritingTask.Create(
            WritingTaskId.From(Guid.NewGuid()), tenantId, "Task 2 - Opinion Essay", WritingTaskType.Task2, ReadingTrack.Academic, "p", null,
            250, 400, null, Now).Value);
        var handler = new CreateWritingTaskCommandHandler(tasks, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateWritingTaskCommand(
                tenantId, "Task 2 - Opinion Essay", WritingTaskType.Task2, ReadingTrack.Academic, "Different prompt", null, 250, 400, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_WRITING_TASK_TITLE_TAKEN", result.Error.Code);
        Assert.Single(tasks.Tasks);
    }
}
