using IELTSLms.Application.AI.Orchestration;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace IELTSLms.Application.Tests.AI.Orchestration;

public class WorkflowEngineTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task ExecuteAsync_ShouldRunSequentialSteps_AndCompleteRun()
    {
        // Arrange
        var repos = new FakeWorkflowRunRepository();
        var uowMock = new Mock<IELTSLms.Abstractions.Persistence.IUnitOfWork>();
        var clockMock = new Mock<IELTSLms.Abstractions.Time.IClock>();
        clockMock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);
        var guidMock = new Mock<IELTSLms.Abstractions.Time.IGuidProvider>();
        guidMock.Setup(g => g.NewGuid()).Returns(Guid.NewGuid());

        var engine = new WorkflowEngine(
            repos,
            uowMock.Object,
            clockMock.Object,
            guidMock.Object,
            NullLogger<WorkflowEngine>.Instance);

        var context = new WorkflowContext(TenantId, UserId, new Dictionary<string, object?>());

        var stepsExecuted = new List<string>();
        var def = new WorkflowDefinition("test-flow",
        [
            new StepDefinition("s1", StepKind.Sequential, (ctx, ct) => { stepsExecuted.Add("s1"); return Task.FromResult(StepResult.Success()); }),
            new StepDefinition("s2", StepKind.Sequential, (ctx, ct) => { stepsExecuted.Add("s2"); return Task.FromResult(StepResult.Success()); }),
        ]);

        // Act
        var result = await engine.ExecuteAsync(def, context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        stepsExecuted.ShouldBe(["s1", "s2"]);
        
        var run = repos.Runs.Single();
        run.Status.ShouldBe(WorkflowRunStatus.Completed);
        run.WorkflowKey.ShouldBe("test-flow");
    }

    [Fact]
    public async Task ExecuteAsync_OnFailure_ShouldRunCompensationsInReverse()
    {
        // Arrange
        var repos = new FakeWorkflowRunRepository();
        var uowMock = new Mock<IELTSLms.Abstractions.Persistence.IUnitOfWork>();
        var clockMock = new Mock<IELTSLms.Abstractions.Time.IClock>();
        clockMock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);
        var guidMock = new Mock<IELTSLms.Abstractions.Time.IGuidProvider>();
        guidMock.Setup(g => g.NewGuid()).Returns(Guid.NewGuid());

        var engine = new WorkflowEngine(
            repos,
            uowMock.Object,
            clockMock.Object,
            guidMock.Object,
            NullLogger<WorkflowEngine>.Instance);

        var context = new WorkflowContext(TenantId, UserId, new Dictionary<string, object?>());

        var stepsExecuted = new List<string>();
        var compensations = new List<string>();

        var def = new WorkflowDefinition("test-flow",
        [
            new StepDefinition("s1", StepKind.Sequential, 
                (ctx, ct) => { stepsExecuted.Add("s1"); return Task.FromResult(StepResult.Success()); },
                Compensate: (ctx, ct) => { compensations.Add("s1"); return Task.CompletedTask; }),
            
            new StepDefinition("s2", StepKind.Sequential, 
                (ctx, ct) => { stepsExecuted.Add("s2"); return Task.FromResult(StepResult.Failure("ERR")); },
                Compensate: (ctx, ct) => { compensations.Add("s2"); return Task.CompletedTask; }),
            
            new StepDefinition("s3", StepKind.Sequential, 
                (ctx, ct) => { stepsExecuted.Add("s3"); return Task.FromResult(StepResult.Success()); })
        ]);

        // Act
        var result = await engine.ExecuteAsync(def, context);

        // Assert
        result.IsSuccess.ShouldBeTrue(); // The Execute method itself returns success with the run ID even if it failed and compensated.
        // Wait, WorkflowEngine.ExecuteAsync returns Result.Success(run.Id) because the engine orchestrates it. 
        // Wait, if it failed, it still returns Success(run.Id)?
        // Let's check: Yes, the code says "return Result.Success(run.Id);" for both success and failure, Wait, actually no!
        // Looking at WorkflowEngine: it throws exceptions, or does it? 
        // The implementation: HandleFailureAsync is called, then it returns. But ExecuteStepsAsync doesn't return anything. 
        // Then ExecuteAsync says: `await ExecuteStepsAsync(...); return Result.Success(run.Id);`.
        // So yes, it returns the run ID.

        stepsExecuted.ShouldBe(["s1", "s2"]); // s3 shouldn't run.
        compensations.ShouldBe(["s1"]); // only s1 succeeded, so only s1 is compensated.

        var run = repos.Runs.Single();
        run.Status.ShouldBe(WorkflowRunStatus.Compensated); // Since compensation succeeded.
    }
}
