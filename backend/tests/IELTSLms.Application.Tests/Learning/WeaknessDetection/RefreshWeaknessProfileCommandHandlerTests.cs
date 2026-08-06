using IELTSLms.Application.Learning.WeaknessDetection.Commands.RefreshWeaknessProfile;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.WeaknessDetection;

/// <summary>Also covers the sprint's required "unit-test the minimum-data-threshold gate" acceptance
/// criterion — indirectly, through this handler's public behavior, since the gate itself
/// (<c>ProgressAnalysisService.IsSufficient</c>) is <see langword="internal"/> and this test project has
/// no <c>InternalsVisibleTo</c> into Application (same constraint every other handler test here already
/// works under, e.g. asserting on <c>Error.Code</c> string literals rather than internal error types).</summary>
public sealed class RefreshWeaknessProfileCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static void SeedSufficientActivity(FakeProgressTrackingRepository progressTracking, Guid tenantId, Guid userId)
    {
        var grammar = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Grammar, Now);
        grammar.RecordActivity(Now);
        grammar.RecordActivity(Now);
        grammar.RecordActivity(Now);
        progressTracking.Seed(grammar);

        var reading = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Reading, Now);
        reading.RecordActivity(Now);
        reading.RecordActivity(Now);
        progressTracking.Seed(reading);
    }

    private static (RefreshWeaknessProfileCommandHandler Handler, FakeAiOperationRepository Operations, FakeBackgroundJobService BackgroundJobs,
        FakeEntitlementService Entitlements, FakeQuotaService Quotas, FakeProgressTrackingRepository ProgressTracking) CreateHandler()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var operations = new FakeAiOperationRepository();
        var backgroundJobs = new FakeBackgroundJobService();
        var entitlements = new FakeEntitlementService();
        var quotas = new FakeQuotaService();

        var handler = new RefreshWeaknessProfileCommandHandler(
            progressTracking, operations, new FakeUnitOfWork(), entitlements, quotas, backgroundJobs,
            new FakeClock(Now), new FakeGuidProvider());

        return (handler, operations, backgroundJobs, entitlements, quotas, progressTracking);
    }

    [Fact]
    public async Task Below_the_minimum_data_threshold_fails_without_touching_entitlement_quota_or_ai_operation()
    {
        var (handler, operations, backgroundJobs, _, _, _) = CreateHandler();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new RefreshWeaknessProfileCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_WEAKNESS_PROFILE_INSUFFICIENT_DATA", result.Error.Code);
        Assert.Empty(operations.Operations);
        Assert.Equal(0, backgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Above_threshold_but_not_entitled_fails()
    {
        var (handler, _, _, entitlements, _, progressTracking) = CreateHandler();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SeedSufficientActivity(progressTracking, tenantId, userId);
        entitlements.HasFeature = false;

        var result = await handler.Handle(new RefreshWeaknessProfileCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
    }

    [Fact]
    public async Task Above_threshold_but_quota_exhausted_fails()
    {
        var (handler, _, _, _, quotas, progressTracking) = CreateHandler();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SeedSufficientActivity(progressTracking, tenantId, userId);
        quotas.Allowed = false;

        var result = await handler.Handle(new RefreshWeaknessProfileCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
    }

    [Fact]
    public async Task Above_threshold_entitled_and_within_quota_enqueues_an_operation_and_the_progress_analysis_job()
    {
        var (handler, operations, backgroundJobs, _, _, progressTracking) = CreateHandler();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SeedSufficientActivity(progressTracking, tenantId, userId);

        var result = await handler.Handle(new RefreshWeaknessProfileCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Single(operations.Operations);
        Assert.Equal(1, backgroundJobs.EnqueueCallCount);
    }
}
