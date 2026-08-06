using IELTSLms.Domain.AI;
using Shouldly;

namespace IELTSLms.Domain.Tests.AI;

public class WorkflowRunTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Begin_ShouldCreateValidWorkflowRun()
    {
        // Arrange
        var id = WorkflowRunId.From(Guid.NewGuid());
        var workflowKey = "monthly-mock-exam";
        var triggerRef = "exam-123";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = WorkflowRun.Begin(id, TenantId, UserId, workflowKey, triggerRef, now);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var run = result.Value;
        run.Id.ShouldBe(id);
        run.TenantId.ShouldBe(TenantId);
        run.UserId.ShouldBe(UserId);
        run.WorkflowKey.ShouldBe(workflowKey);
        run.TriggerRef.ShouldBe(triggerRef);
        run.Status.ShouldBe(WorkflowRunStatus.Running);
        run.StartedAt.ShouldBe(now);
        run.CompletedAt.ShouldBeNull();
        run.StepTraceJson.ShouldBeNull();
    }

    [Fact]
    public void Complete_ShouldSetStatusAndTrace()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var run = WorkflowRun.Begin(WorkflowRunId.From(Guid.NewGuid()), TenantId, UserId, "wk", "tr", now).Value;
        var completeTime = now.AddMinutes(1);
        var traceJson = "[{\"step\":\"1\"}]";

        // Act
        run.Complete(traceJson, completeTime);

        // Assert
        run.Status.ShouldBe(WorkflowRunStatus.Completed);
        run.CompletedAt.ShouldBe(completeTime);
        run.StepTraceJson.ShouldBe(traceJson);
    }

    [Fact]
    public void Fail_ShouldSetStatusAndTrace()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var run = WorkflowRun.Begin(WorkflowRunId.From(Guid.NewGuid()), TenantId, UserId, "wk", "tr", now).Value;
        var failTime = now.AddMinutes(1);
        var traceJson = "[{\"step\":\"1\",\"error\":\"boom\"}]";

        // Act
        run.Fail(traceJson, failTime);

        // Assert
        run.Status.ShouldBe(WorkflowRunStatus.Failed);
        run.CompletedAt.ShouldBe(failTime);
        run.StepTraceJson.ShouldBe(traceJson);
    }

    [Fact]
    public void Compensate_ShouldSetStatusAndTrace()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var run = WorkflowRun.Begin(WorkflowRunId.From(Guid.NewGuid()), TenantId, UserId, "wk", "tr", now).Value;
        var compTime = now.AddMinutes(1);
        var traceJson = "[{\"step\":\"1\",\"error\":\"boom\"}]";

        // Act
        run.Compensate(traceJson, compTime);

        // Assert
        run.Status.ShouldBe(WorkflowRunStatus.Compensated);
        run.CompletedAt.ShouldBe(compTime);
        run.StepTraceJson.ShouldBe(traceJson);
    }
}
