using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories.Billing;

internal sealed class PaymentMethodRepository(ApplicationDbContext dbContext)
    : RepositoryBase<PaymentMethod, PaymentMethodId>(dbContext), IPaymentMethodRepository
{
    public async Task<IReadOnlyList<PaymentMethod>> ListByCustomerAsync(
        BillingCustomerId billingCustomerId, CancellationToken cancellationToken = default) =>
        await Set.Where(m => m.BillingCustomerId == billingCustomerId).ToListAsync(cancellationToken).ConfigureAwait(false);
}
