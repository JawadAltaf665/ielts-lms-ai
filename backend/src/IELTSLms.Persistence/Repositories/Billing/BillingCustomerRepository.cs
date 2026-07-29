using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class BillingCustomerRepository(ApplicationDbContext dbContext)
    : RepositoryBase<BillingCustomer, BillingCustomerId>(dbContext), IBillingCustomerRepository
{
    public async Task<BillingCustomer?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken).ConfigureAwait(false);

    public async Task<BillingCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(c => c.StripeCustomerId == stripeCustomerId, cancellationToken).ConfigureAwait(false);
}
