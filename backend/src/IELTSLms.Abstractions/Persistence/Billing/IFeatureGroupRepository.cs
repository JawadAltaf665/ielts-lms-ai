using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="FeatureGroup"/> (ERD ENT-039D).</summary>
public interface IFeatureGroupRepository : IRepository<FeatureGroup, FeatureGroupId>
{
    Task<IReadOnlyList<FeatureGroup>> ListAllAsync(CancellationToken cancellationToken = default);
}
