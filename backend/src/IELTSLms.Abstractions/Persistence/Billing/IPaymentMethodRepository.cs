using IELTSLms.Domain.Billing;

namespace IELTSLms.Abstractions.Persistence.Billing;

/// <summary>Aggregate access for <see cref="PaymentMethod"/> (ERD ENT-039M).</summary>
public interface IPaymentMethodRepository : IRepository<PaymentMethod, PaymentMethodId>
{
    Task<IReadOnlyList<PaymentMethod>> ListByCustomerAsync(BillingCustomerId billingCustomerId, CancellationToken cancellationToken = default);
}
