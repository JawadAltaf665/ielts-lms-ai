using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="QuotaDefinition"/> (ERD ENT-039H). Global catalog, not tenant-scoped.</summary>
public interface IQuotaDefinitionRepository : IRepository<QuotaDefinition, QuotaDefinitionId>
{
    Task<QuotaDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuotaDefinition>> ListAllAsync(CancellationToken cancellationToken = default);
}
