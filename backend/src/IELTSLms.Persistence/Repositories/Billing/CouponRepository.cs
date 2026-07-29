using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class CouponRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Coupon, CouponId>(dbContext), ICouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(c => c.Code == code, cancellationToken).ConfigureAwait(false);
}
