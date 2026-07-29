using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Feature"/> (ERD ENT-039E). Global catalog, not tenant-scoped.</summary>
public interface IFeatureRepository : IRepository<Feature, FeatureId>
{
    Task<Feature?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feature>> ListAllAsync(CancellationToken cancellationToken = default);
}
