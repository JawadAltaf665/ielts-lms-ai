using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="TenantQuota"/> — tenant-specific overrides (ERD ENT-039J).</summary>
public interface ITenantQuotaRepository : IRepository<TenantQuota, TenantQuotaId>
{
    Task<TenantQuota?> GetAsync(Guid tenantId, QuotaDefinitionId quotaDefinitionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantQuota>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
