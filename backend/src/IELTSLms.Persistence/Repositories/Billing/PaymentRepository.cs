using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PaymentRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Payment, PaymentId>(dbContext), IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.InvoiceId == invoiceId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Payment>> ListByTenantAsync(
        Guid tenantId, int skip, int take, CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
}
