using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="BillingCustomer"/> (ERD ENT-039L). One per tenant.</summary>
public interface IBillingCustomerRepository : IRepository<BillingCustomer, BillingCustomerId>
{
    Task<BillingCustomer?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<BillingCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
}
