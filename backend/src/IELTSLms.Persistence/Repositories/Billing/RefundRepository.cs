using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class RefundRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Refund, RefundId>(dbContext), IRefundRepository
{
    public async Task<IReadOnlyList<Refund>> ListByPaymentAsync(PaymentId paymentId, CancellationToken cancellationToken = default) =>
        await Set.Where(r => r.PaymentId == paymentId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Refund>> ListByTenantAsync(
        Guid tenantId, int skip, int take, CancellationToken cancellationToken = default) =>
        await Set.Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip(skip).Take(take)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Refund>> ListBetweenAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        await Set.Where(r => r.CreatedAtUtc >= from && r.CreatedAtUtc < to).ToListAsync(cancellationToken).ConfigureAwait(false);
}
