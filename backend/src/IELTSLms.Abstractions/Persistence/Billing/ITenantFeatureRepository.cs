using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="TenantFeature"/> — tenant-specific overrides (ERD ENT-039G).</summary>
public interface ITenantFeatureRepository : IRepository<TenantFeature, TenantFeatureId>
{
    Task<TenantFeature?> GetAsync(Guid tenantId, FeatureId featureId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantFeature>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
