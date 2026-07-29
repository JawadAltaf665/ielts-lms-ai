using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Payment"/> (ERD ENT-042).</summary>
public interface IPaymentRepository : IRepository<Payment, PaymentId>
{
    Task<IReadOnlyList<Payment>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Payments attempted within the window, across every tenant, for reporting (ADR-032). Requires an active tenant-filter bypass.</summary>
    Task<IReadOnlyList<Payment>> ListBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
