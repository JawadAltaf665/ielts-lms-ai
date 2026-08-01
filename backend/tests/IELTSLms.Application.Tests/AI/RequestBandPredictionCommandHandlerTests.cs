using IELTSLms.Application.AI.RequestBandPrediction;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class RequestBandPredictionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static RequestBandPredictionCommandHandler CreateHandler(
        FakeBandPredictionRepository predictions, FakeAiOperationRepository operations, FakeUnitOfWork unitOfWork,
        FakeEntitlementService entitlements, FakeQuotaService quotas, FakeBackgroundJobService backgroundJobs) =>
        new(predictions, operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task Not_entitled_is_rejected_without_creating_a_prediction_or_operation()
    {
        var predictions = new FakeBandPredictionRepository();
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            predictions, operations, new FakeUnitOfWork(), new FakeEntitlementService { HasFeature = false }, new FakeQuotaService(),
            backgroundJobs);

        var result = await handler.Handle(new RequestBandPredictionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
        Assert.Empty(predictions.Predictions);
        Assert.Empty(operations.Operations);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var predictions = new FakeBandPredictionRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            predictions, new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeEntitlementService(),
            new FakeQuotaService { Allowed = false }, backgroundJobs);

        var result = await handler.Handle(new RequestBandPredictionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Empty(predictions.Predictions);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Entitled_and_within_quota_creates_a_pending_prediction_linked_to_a_queued_operation()
    {
        var predictions = new FakeBandPredictionRepository();
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(predictions, operations, unitOfWork, new FakeEntitlementService(), new FakeQuotaService(), backgroundJobs);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new RequestBandPredictionCommand(Guid.NewGuid(), userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var prediction = Assert.Single(predictions.Predictions);
        Assert.Equal(userId, prediction.UserId);
        Assert.Equal(BandPredictionStatus.Pending, prediction.Status);
        Assert.Equal(result.Value.BandPredictionId, prediction.Id.Value);

        var operation = Assert.Single(operations.Operations);
        Assert.Equal(result.Value.OperationId, operation.Id.Value);
        Assert.Equal(prediction.AiOperationId, operation.Id);
    }
}
