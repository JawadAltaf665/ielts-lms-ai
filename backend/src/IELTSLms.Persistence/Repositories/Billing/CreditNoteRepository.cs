using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class CreditNoteRepository(ApplicationDbContext dbContext)
    : RepositoryBase<CreditNote, CreditNoteId>(dbContext), ICreditNoteRepository
{
    public async Task<IReadOnlyList<CreditNote>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default) =>
        await Set.Where(c => c.InvoiceId == invoiceId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
