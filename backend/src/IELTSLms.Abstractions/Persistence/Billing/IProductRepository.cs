using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Product"/> (ERD ENT-039A). Global catalog, not tenant-scoped.</summary>
public interface IProductRepository : IRepository<Product, ProductId>
{
    Task<Product?> GetByCodeAsync(string internalCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListActiveAsync(CancellationToken cancellationToken = default);
}
