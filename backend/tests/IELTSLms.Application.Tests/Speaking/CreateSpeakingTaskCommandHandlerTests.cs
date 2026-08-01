using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class CreateSpeakingTaskCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_task()
    {
        var tasks = new FakeSpeakingTaskRepository();
        var handler = new CreateSpeakingTaskCommandHandler(tasks, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateSpeakingTaskCommand(tenantId, "Part 2 - Describe a place", SpeakingPart.Part2, "Describe a place.", null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.False(result.Value.IsGlobal);
        Assert.Single(tasks.Tasks);
    }

    [Fact]
    public async Task Rejects_a_duplicate_title_within_the_same_tenant()
    {
        var tasks = new FakeSpeakingTaskRepository();
        var tenantId = Guid.NewGuid();
        tasks.Seed(SpeakingTask.Create(
            SpeakingTaskId.From(Guid.NewGuid()), tenantId, "Part 1", SpeakingPart.Part1, "prompt", null, Now).Value);
        var handler = new CreateSpeakingTaskCommandHandler(tasks, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateSpeakingTaskCommand(tenantId, "Part 1", SpeakingPart.Part1, "another prompt", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_TASK_TITLE_TAKEN", result.Error.Code);
        Assert.Single(tasks.Tasks);
    }
}
