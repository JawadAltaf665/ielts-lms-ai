using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeQuotaDefinitionRepository : IQuotaDefinitionRepository
{
    private readonly List<QuotaDefinition> _definitions = [];

    public void Seed(QuotaDefinition definition) => _definitions.Add(definition);

    public Task<QuotaDefinition?> GetByIdAsync(QuotaDefinitionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_definitions.FirstOrDefault(d => d.Id == id));

    public void Add(QuotaDefinition aggregate) => _definitions.Add(aggregate);

    public void Remove(QuotaDefinition aggregate) => _definitions.Remove(aggregate);

    public Task<QuotaDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_definitions.FirstOrDefault(d => d.Code == code));

    public Task<IReadOnlyList<QuotaDefinition>> ListAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<QuotaDefinition>>(_definitions.ToArray());
}

internal sealed class FakePlanQuotaRepository : IPlanQuotaRepository
{
    private readonly List<PlanQuota> _planQuotas = [];

    public void Seed(PlanQuota planQuota) => _planQuotas.Add(planQuota);

    public Task<PlanQuota?> GetByIdAsync(PlanQuotaId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_planQuotas.FirstOrDefault(p => p.Id == id));

    public void Add(PlanQuota aggregate) => _planQuotas.Add(aggregate);

    public void Remove(PlanQuota aggregate) => _planQuotas.Remove(aggregate);

    public Task<IReadOnlyList<PlanQuota>> ListByPlanAsync(PlanId planId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PlanQuota>>(_planQuotas.Where(p => p.PlanId == planId).ToArray());

    public Task<PlanQuota?> GetAsync(PlanId planId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_planQuotas.FirstOrDefault(p => p.PlanId == planId && p.QuotaDefinitionId == quotaDefinitionId));
}

internal sealed class FakeTenantQuotaRepository : ITenantQuotaRepository
{
    private readonly List<TenantQuota> _tenantQuotas = [];

    public Task<TenantQuota?> GetByIdAsync(TenantQuotaId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenantQuotas.FirstOrDefault(t => t.Id == id));

    public void Add(TenantQuota aggregate) => _tenantQuotas.Add(aggregate);

    public void Remove(TenantQuota aggregate) => _tenantQuotas.Remove(aggregate);

    public Task<TenantQuota?> GetAsync(Guid tenantId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenantQuotas.FirstOrDefault(t => t.TenantId == tenantId && t.QuotaDefinitionId == quotaDefinitionId));

    public Task<IReadOnlyList<TenantQuota>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TenantQuota>>(_tenantQuotas.Where(t => t.TenantId == tenantId).ToArray());
}

internal sealed class FakeUsageRecordRepository : IUsageRecordRepository
{
    private readonly List<UsageRecord> _records = [];

    public Task<UsageRecord?> GetByIdAsync(UsageRecordId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_records.FirstOrDefault(u => u.Id == id));

    public void Add(UsageRecord aggregate) => _records.Add(aggregate);

    public void Remove(UsageRecord aggregate) => _records.Remove(aggregate);

    public Task<UsageRecord?> GetAsync(
        Guid tenantId, QuotaDefinitionId quotaDefinitionId, DateTimeOffset periodStart, CancellationToken cancellationToken = default) =>
        Task.FromResult(_records.FirstOrDefault(
            u => u.TenantId == tenantId && u.QuotaDefinitionId == quotaDefinitionId && u.PeriodStart == periodStart));

    public Task<IReadOnlyList<UsageRecord>> ListByTenantAsync(
        Guid tenantId, DateTimeOffset periodStart, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UsageRecord>>(
            _records.Where(u => u.TenantId == tenantId && u.PeriodStart == periodStart).ToArray());

    public Task<IReadOnlyList<UsageRecord>> ListEndingBeforeAsync(DateTimeOffset before, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<UsageRecord>>(_records.Where(u => u.PeriodEnd < before).ToArray());
}
