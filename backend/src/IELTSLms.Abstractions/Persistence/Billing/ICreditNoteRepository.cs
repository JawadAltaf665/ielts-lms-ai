using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="CreditNote"/> (ERD ENT-039Q).</summary>
public interface ICreditNoteRepository : IRepository<CreditNote, CreditNoteId>
{
    Task<IReadOnlyList<CreditNote>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
}
