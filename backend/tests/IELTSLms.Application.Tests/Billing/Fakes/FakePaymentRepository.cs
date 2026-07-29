using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakePaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = [];

    public void Seed(Payment payment) => _payments.Add(payment);

    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_payments.FirstOrDefault(p => p.Id == id));

    public void Add(Payment aggregate) => _payments.Add(aggregate);

    public void Remove(Payment aggregate) => _payments.Remove(aggregate);

    public Task<IReadOnlyList<Payment>> ListByInvoiceAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Payment>>(_payments.Where(p => p.InvoiceId == invoiceId).ToArray());

    public Task<IReadOnlyList<Payment>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Payment>>(_payments.Where(p => p.TenantId == tenantId).Skip(skip).Take(take).ToArray());

    public Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_payments.Count(p => p.TenantId == tenantId));

    public Task<IReadOnlyList<Payment>> ListBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Payment>>(_payments.Where(p => p.CreatedAtUtc >= from && p.CreatedAtUtc < to).ToArray());
}
