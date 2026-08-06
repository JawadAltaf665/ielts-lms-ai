using IELTSLms.Application.AI.Orchestration.Queries.GetWorkflowRun;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using Shouldly;

namespace IELTSLms.Application.Tests.AI.Orchestration;

public class GetWorkflowRunQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ShouldReturnRun_WhenFound()
    {
        // Arrange
        var repos = new FakeWorkflowRunRepository();
        var runId = Guid.NewGuid();
        var run = WorkflowRun.Begin(WorkflowRunId.From(runId), TenantId, UserId, "monthly-mock-exam", "123", DateTimeOffset.UtcNow).Value;
        repos.Seed(run);

        var handler = new GetWorkflowRunQueryHandler(repos);
        var query = new GetWorkflowRunQuery(TenantId, UserId, runId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(runId);
        result.Value.Status.ShouldBe(WorkflowRunStatus.Running.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        var repos = new FakeWorkflowRunRepository();
        var handler = new GetWorkflowRunQueryHandler(repos);
        var query = new GetWorkflowRunQuery(TenantId, UserId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("NOT_FOUND_WORKFLOW_RUN");
    }
}
