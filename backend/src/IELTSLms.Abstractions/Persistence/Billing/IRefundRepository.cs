using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Refund"/> (ERD ENT-039S).</summary>
public interface IRefundRepository : IRepository<Refund, RefundId>
{
    Task<IReadOnlyList<Refund>> ListByPaymentAsync(PaymentId paymentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Refund>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>Refunds issued within the window, across every tenant, for reporting (ADR-032). Requires an active tenant-filter bypass.</summary>
    Task<IReadOnlyList<Refund>> ListBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
}
