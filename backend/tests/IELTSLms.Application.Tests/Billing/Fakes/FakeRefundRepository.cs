using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeRefundRepository : IRefundRepository
{
    private readonly List<Refund> _refunds = [];

    public void Seed(Refund refund) => _refunds.Add(refund);

    public Task<Refund?> GetByIdAsync(RefundId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_refunds.FirstOrDefault(r => r.Id == id));

    public void Add(Refund aggregate) => _refunds.Add(aggregate);

    public void Remove(Refund aggregate) => _refunds.Remove(aggregate);

    public Task<IReadOnlyList<Refund>> ListByPaymentAsync(PaymentId paymentId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Refund>>(_refunds.Where(r => r.PaymentId == paymentId).ToArray());

    public Task<IReadOnlyList<Refund>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Refund>>(_refunds.Where(r => r.TenantId == tenantId).Skip(skip).Take(take).ToArray());

    public Task<IReadOnlyList<Refund>> ListBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Refund>>(_refunds.Where(r => r.CreatedAtUtc >= from && r.CreatedAtUtc < to).ToArray());
}
