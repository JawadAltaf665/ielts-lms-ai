using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class ProductRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Product, ProductId>(dbContext), IProductRepository
{
    public async Task<Product?> GetByCodeAsync(string internalCode, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(p => p.InternalCode == internalCode, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Product>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.IsActive).ToListAsync(cancellationToken).ConfigureAwait(false);
}
