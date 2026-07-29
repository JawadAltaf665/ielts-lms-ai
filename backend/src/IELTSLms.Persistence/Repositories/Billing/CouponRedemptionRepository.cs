using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class CouponRedemptionRepository(ApplicationDbContext dbContext)
    : RepositoryBase<CouponRedemption, CouponRedemptionId>(dbContext), ICouponRedemptionRepository
{
    public async Task<CouponRedemption?> GetActiveBySubscriptionAsync(
        SubscriptionId subscriptionId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(r => r.SubscriptionId == subscriptionId && r.IsActive, cancellationToken).ConfigureAwait(false);

    public async Task<int> CountByCouponAsync(CouponId couponId, CancellationToken cancellationToken = default) =>
        await Set.CountAsync(r => r.CouponId == couponId, cancellationToken).ConfigureAwait(false);

    public async Task<int> CountBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default) =>
        await Set.CountAsync(r => r.CreatedAtUtc >= from && r.CreatedAtUtc < to, cancellationToken).ConfigureAwait(false);
}
