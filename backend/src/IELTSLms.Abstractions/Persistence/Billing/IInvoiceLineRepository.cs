using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="InvoiceLine"/> (ERD ENT-039O).</summary>
public interface IInvoiceLineRepository : IRepository<InvoiceLine, InvoiceLineId>
{
    Task<IReadOnlyList<InvoiceLine>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
}
