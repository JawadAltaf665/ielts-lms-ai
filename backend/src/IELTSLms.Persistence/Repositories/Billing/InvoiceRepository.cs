using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class InvoiceRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Invoice, InvoiceId>(dbContext), IInvoiceRepository
{
    public async Task<IReadOnlyList<Invoice>> ListByTenantAsync(
        Guid tenantId, int skip, int take, CancellationToken cancellationToken = default) =>
        await Set.Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.CountAsync(i => i.TenantId == tenantId, cancellationToken).ConfigureAwait(false);

    public async Task<Invoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(i => i.StripeInvoiceId == stripeInvoiceId, cancellationToken).ConfigureAwait(false);

    public async Task<int> CountAllForTenantIncludingDeletedAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.IgnoreQueryFilters().CountAsync(i => i.TenantId == tenantId, cancellationToken).ConfigureAwait(false);
}
