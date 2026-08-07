using IELTSLms.Application.Assessment;
using IELTSLms.Application.Assessment.Queries.GetOrchestratedReview;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Assessment;
using Moq;
using Shouldly;

namespace IELTSLms.Application.Tests.Assessment.Queries;

public class GetOrchestratedReviewQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ShouldReturnReview_WhenFound()
    {
        // Arrange
        var mockExamsMock = new Mock<IELTSLms.Abstractions.Persistence.Assessment.IMockExamRepository>();
        var mockExamId = Guid.NewGuid();
        var mockExam = MonthlyMockExam.Start(MockExamId.From(mockExamId), TenantId, UserId, DateTimeOffset.UtcNow);
        
        mockExamsMock
            .Setup(m => m.GetByIdAsync(MockExamId.From(mockExamId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockExam);

        var repos = new FakeWorkflowRunRepository();
        var run = WorkflowRun.Begin(WorkflowRunId.From(Guid.NewGuid()), TenantId, UserId, "monthly-mock-exam", mockExamId.ToString(), DateTimeOffset.UtcNow).Value;
        repos.Seed(run);

        var handler = new GetOrchestratedReviewQueryHandler(mockExamsMock.Object, repos);
        var query = new GetOrchestratedReviewQuery(TenantId, UserId, mockExamId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.WorkflowRunId.ShouldBe(run.Id.Value);
        result.Value.Status.ShouldBe(WorkflowRunStatus.Running.ToString());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMockExamMissing()
    {
        // Arrange
        var mockExamsMock = new Mock<IELTSLms.Abstractions.Persistence.Assessment.IMockExamRepository>();
        mockExamsMock
            .Setup(m => m.GetByIdAsync(It.IsAny<MockExamId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlyMockExam?)null);

        var repos = new FakeWorkflowRunRepository();
        var handler = new GetOrchestratedReviewQueryHandler(mockExamsMock.Object, repos);
        var query = new GetOrchestratedReviewQuery(TenantId, UserId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("NOT_FOUND_MOCK_EXAM");
    }
}
