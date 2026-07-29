using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Payment"/> (ERD ENT-042).</summary>
public interface IPaymentRepository : IRepository<Payment, PaymentId>
{
    Task<IReadOnlyList<Payment>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default);
}
