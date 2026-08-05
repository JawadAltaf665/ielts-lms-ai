using IELTSLms.Application.AI.RequestReadingCoaching;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class RequestReadingCoachingCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static RequestReadingCoachingCommandHandler CreateHandler(
        FakeAiOperationRepository operations, FakeUnitOfWork unitOfWork, FakeEntitlementService entitlements,
        FakeQuotaService quotas, FakeBackgroundJobService backgroundJobs) =>
        new(operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task Not_entitled_is_rejected_without_touching_quota_or_enqueueing()
    {
        var operations = new FakeAiOperationRepository();
        var quotas = new FakeQuotaService();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            operations, new FakeUnitOfWork(), new FakeEntitlementService { HasFeature = false }, quotas, backgroundJobs);

        var result = await handler.Handle(
            new RequestReadingCoachingCommand(Guid.NewGuid(), Guid.NewGuid(), "how do I skim a passage?"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
        Assert.Equal(0, quotas.ConsumeCallCount);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            operations, new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService { Allowed = false }, backgroundJobs);

        var result = await handler.Handle(
            new RequestReadingCoachingCommand(Guid.NewGuid(), Guid.NewGuid(), "how do I skim a passage?"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(SharedKernel.Results.ErrorType.RateLimited, result.Error.Type);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Entitled_and_within_quota_enqueues_the_job_and_returns_queued()
    {
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(operations, unitOfWork, new FakeEntitlementService(), new FakeQuotaService(), backgroundJobs);

        var result = await handler.Handle(
            new RequestReadingCoachingCommand(Guid.NewGuid(), Guid.NewGuid(), "how do I skim a passage?"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var operation = Assert.Single(operations.Operations);
        Assert.Equal(AiOperationStatus.Queued, operation.Status);
        Assert.Equal("how do I skim a passage?", operation.InputText);
    }
}
