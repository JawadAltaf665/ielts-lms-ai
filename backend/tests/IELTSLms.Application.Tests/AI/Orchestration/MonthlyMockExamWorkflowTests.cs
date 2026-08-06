using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.Orchestration;
using IELTSLms.Application.AI.Orchestration.Workflows;
using IELTSLms.Application.AI.RequestBandPrediction;
using IELTSLms.Application.Learning.StudyPlanner;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Moq;
using Shouldly;

namespace IELTSLms.Application.Tests.AI.Orchestration;

public class MonthlyMockExamWorkflowTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    
    // For this test we can use Moq for the dependencies to just evaluate step execution.
    // However, DailyLearningWorkflowRunner is a concrete class in Application, so we'd need
    // either a fake or to mock it. We will just test the build method returns the correct steps,
    // and manually execute them to verify context flow.

    [Fact]
    public void Build_ShouldReturnThreeStepsWithCorrectKeys()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var clockMock = new Mock<IClock>();
        
        // Act
        var def = MonthlyMockExamWorkflow.Build(TenantId, UserId, senderMock.Object, null!, clockMock.Object);

        // Assert
        def.Key.ShouldBe(MonthlyMockExamWorkflow.Key);
        def.Steps.Count.ShouldBe(4); // 1a, 1b, 2, 3
        
        def.Steps[0].StepKey.ShouldBe("reading-score");
        def.Steps[0].Kind.ShouldBe(StepKind.Parallel);
        
        def.Steps[1].StepKey.ShouldBe("listening-score");
        def.Steps[1].Kind.ShouldBe(StepKind.Parallel);

        def.Steps[2].StepKey.ShouldBe("band-predictor");
        def.Steps[2].Kind.ShouldBe(StepKind.Sequential);

        def.Steps[3].StepKey.ShouldBe("learning-recommendation");
        def.Steps[3].Kind.ShouldBe(StepKind.Conditional);
    }

    [Fact]
    public async Task BandPredictorStep_ShouldSendRequestCommand_AndUpdateContext()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var clockMock = new Mock<IClock>();
        
        var expectedPredictionId = Guid.NewGuid();
        var expectedOperationId = Guid.NewGuid();
        senderMock
            .Setup(s => s.Send(It.IsAny<RequestBandPredictionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new BandPredictionAcceptedResponse(expectedPredictionId, expectedOperationId, "queued")));

        var def = MonthlyMockExamWorkflow.Build(TenantId, UserId, senderMock.Object, null!, clockMock.Object);
        var bandPredictorStep = def.Steps[2];
        var ctx = new WorkflowContext(TenantId, UserId, new Dictionary<string, object?>());

        // Act
        var result = await bandPredictorStep.Execute(ctx, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        ctx.Get<Guid>(MonthlyMockExamWorkflowKeys.BandPredictionId).ShouldBe(expectedPredictionId);
    }
}
