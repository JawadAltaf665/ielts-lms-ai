using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="Coupon"/> (ERD ENT-043). Global catalog, not tenant-scoped.</summary>
public interface ICouponRepository : IRepository<Coupon, CouponId>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
