using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class InvoiceLineRepository(ApplicationDbContext dbContext)
    : RepositoryBase<InvoiceLine, InvoiceLineId>(dbContext), IInvoiceLineRepository
{
    public async Task<IReadOnlyList<InvoiceLine>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default) =>
        await Set.Where(l => l.InvoiceId == invoiceId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
