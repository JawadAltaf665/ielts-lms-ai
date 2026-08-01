using IELTSLms.Application.Speaking.Commands.SubmitSpeakingAttempt;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class SubmitSpeakingAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, Guid UserId, SpeakingAttempt Attempt) SeedAttempt(
        FakeSpeakingAttemptRepository attempts, int durationSeconds = 45)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var attempt = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, userId, SpeakingTaskId.From(Guid.NewGuid()),
            FileAssetId.From(Guid.NewGuid()), durationSeconds, Now).Value;
        attempts.Seed(attempt);
        return (tenantId, userId, attempt);
    }

    private static SubmitSpeakingAttemptCommandHandler CreateHandler(
        FakeSpeakingAttemptRepository attempts, FakeAiOperationRepository operations, FakeUnitOfWork unitOfWork,
        FakeEntitlementService entitlements, FakeQuotaService quotas, FakeBackgroundJobService backgroundJobs) =>
        new(attempts, operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task Too_short_a_recording_is_rejected_without_enqueueing_anything()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(attempts, durationSeconds: 2);
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            attempts, operations, new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(), backgroundJobs);

        var result = await handler.Handle(new SubmitSpeakingAttemptCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_ATTEMPT_TOO_SHORT", result.Error.Code);
        Assert.Empty(operations.Operations);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
        Assert.Equal(SpeakingAttemptStatus.Recorded, attempt.Status);
    }

    [Fact]
    public async Task An_already_submitted_attempt_is_rejected()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(attempts);
        attempt.Submit(Now);
        var handler = CreateHandler(
            attempts, new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(new SubmitSpeakingAttemptCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_ATTEMPT_NOT_RECORDED", result.Error.Code);
    }

    [Fact]
    public async Task Another_users_attempt_is_not_found_rather_than_forbidden()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, _, attempt) = SeedAttempt(attempts);
        var handler = CreateHandler(
            attempts, new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeEntitlementService(), new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(
            new SubmitSpeakingAttemptCommand(tenantId, Guid.NewGuid(), attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SPEAKING_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task Not_entitled_is_rejected_without_creating_an_operation()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(attempts);
        var operations = new FakeAiOperationRepository();
        var handler = CreateHandler(
            attempts, operations, new FakeUnitOfWork(), new FakeEntitlementService { HasFeature = false }, new FakeQuotaService(),
            new FakeBackgroundJobService());

        var result = await handler.Handle(new SubmitSpeakingAttemptCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
        Assert.Empty(operations.Operations);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(attempts);
        var backgroundJobs = new FakeBackgroundJobService();
        var handler = CreateHandler(
            attempts, new FakeAiOperationRepository(), new FakeUnitOfWork(), new FakeEntitlementService(),
            new FakeQuotaService { Allowed = false }, backgroundJobs);

        var result = await handler.Handle(new SubmitSpeakingAttemptCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Successful_submission_creates_a_queued_operation_and_enqueues_the_evaluation_job()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(attempts);
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(
            attempts, operations, unitOfWork, new FakeEntitlementService(), new FakeQuotaService(), backgroundJobs);

        var result = await handler.Handle(new SubmitSpeakingAttemptCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var operation = Assert.Single(operations.Operations);
        Assert.Equal(result.Value.OperationId, operation.Id.Value);
        Assert.Equal("speaking-evaluation", operation.Kind);
        Assert.Equal(SpeakingAttemptStatus.Submitted, attempt.Status);
    }
}
