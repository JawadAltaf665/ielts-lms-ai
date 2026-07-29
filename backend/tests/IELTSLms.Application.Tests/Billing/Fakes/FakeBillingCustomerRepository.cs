using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;

namespace IELTSLms.Application.Tests.Billing.Fakes;

internal sealed class FakeBillingCustomerRepository : IBillingCustomerRepository
{
    private readonly List<BillingCustomer> _customers = [];

    public void Seed(BillingCustomer customer) => _customers.Add(customer);

    public Task<BillingCustomer?> GetByIdAsync(BillingCustomerId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public void Add(BillingCustomer aggregate) => _customers.Add(aggregate);

    public void Remove(BillingCustomer aggregate) => _customers.Remove(aggregate);

    public Task<BillingCustomer?> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.TenantId == tenantId));

    public Task<BillingCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.StripeCustomerId == stripeCustomerId));
}
