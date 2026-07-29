using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Invoice"/> (ERD ENT-041).</summary>
public interface IInvoiceRepository : IRepository<Invoice, InvoiceId>
{
    Task<IReadOnlyList<Invoice>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>Count of all invoices ever issued to a tenant (including soft-deleted), for sequential invoice numbering (13 §8.1).</summary>
    Task<int> CountAllForTenantIncludingDeletedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
