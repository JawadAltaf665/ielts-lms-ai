using IELTSLms.Application.Billing.Services;
using IELTSLms.Application.Tests.Billing.Fakes;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing;

public sealed class QuotaServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

    private static (QuotaService Service, FakeUsageRecordRepository UsageRecords, FakeUnitOfWork UnitOfWork) BuildService(
        Guid tenantId, long limit)
    {
        var planId = PlanId.From(Guid.NewGuid());
        var quotaDefinition = QuotaDefinition.Create(
            QuotaDefinitionId.From(Guid.NewGuid()), "ai_requests", "AI Requests", "count", QuotaResetCycle.Monthly, Now);

        var quotaDefinitions = new FakeQuotaDefinitionRepository();
        quotaDefinitions.Seed(quotaDefinition);

        var planQuotas = new FakePlanQuotaRepository();
        planQuotas.Seed(PlanQuota.Create(PlanQuotaId.From(Guid.NewGuid()), planId, quotaDefinition.Id, limit, Now).Value);

        var subscription = Subscription.Create(
            SubscriptionId.From(Guid.NewGuid()), tenantId, BillingCustomerId.From(Guid.NewGuid()), planId,
            PriceId.From(Guid.NewGuid()), Now);
        subscription.Activate(Now, Now.AddMonths(1), "sub_123", Now);
        var subscriptions = new FakeSubscriptionRepository();
        subscriptions.Seed(subscription);

        var usageRecords = new FakeUsageRecordRepository();
        var unitOfWork = new FakeUnitOfWork();

        var service = new QuotaService(
            quotaDefinitions, planQuotas, new FakeTenantQuotaRepository(), usageRecords, subscriptions,
            unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        return (service, usageRecords, unitOfWork);
    }

    [Fact]
    public async Task ConsumeAsync_within_limit_succeeds_and_persists_usage()
    {
        var tenantId = Guid.NewGuid();
        var (service, usageRecords, unitOfWork) = BuildService(tenantId, limit: 10);

        var result = await service.ConsumeAsync(tenantId, "ai_requests", 3, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);

        var check = await service.CheckAsync(tenantId, "ai_requests", 1, CancellationToken.None);
        Assert.Equal(3, check.Consumed);
        Assert.Equal(7, check.Remaining);
    }

    [Fact]
    public async Task ConsumeAsync_exceeding_limit_fails_without_persisting()
    {
        var tenantId = Guid.NewGuid();
        var (service, _, unitOfWork) = BuildService(tenantId, limit: 5);

        var result = await service.ConsumeAsync(tenantId, "ai_requests", 10, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CheckAsync_with_unlimited_plan_quota_is_always_allowed()
    {
        var tenantId = Guid.NewGuid();
        var (service, _, _) = BuildService(tenantId, limit: PlanQuota.Unlimited);

        var check = await service.CheckAsync(tenantId, "ai_requests", 1_000_000, CancellationToken.None);

        Assert.True(check.Allowed);
        Assert.Equal(long.MaxValue, check.Remaining);
    }

    [Fact]
    public async Task CheckAsync_for_unknown_quota_code_returns_zero_limit()
    {
        var tenantId = Guid.NewGuid();
        var (service, _, _) = BuildService(tenantId, limit: 10);

        var check = await service.CheckAsync(tenantId, "unknown_quota", 1, CancellationToken.None);

        Assert.False(check.Allowed);
        Assert.Equal(0, check.Limit);
    }
}
