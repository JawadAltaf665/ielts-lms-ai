using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.SetDefaultPaymentMethod;

public sealed class SetDefaultPaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethods, IBillingCustomerRepository billingCustomers,
    IPaymentGateway paymentGateway, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<SetDefaultPaymentMethodCommand>
{
    public async Task<Result> Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var method = await paymentMethods.GetByIdAsync(PaymentMethodId.From(request.PaymentMethodId), cancellationToken)
            .ConfigureAwait(false);
        if (method is null || method.TenantId != request.TenantId)
        {
            return Result.Failure(BillingErrors.PaymentMethodNotFound);
        }

        var customer = await billingCustomers.GetByIdAsync(method.BillingCustomerId, cancellationToken).ConfigureAwait(false);
        if (customer?.StripeCustomerId is null)
        {
            return Result.Failure(BillingErrors.CustomerNotFound);
        }

        var now = clock.UtcNow;
        await paymentGateway
            .SetDefaultPaymentMethodAsync(customer.StripeCustomerId, method.StripePaymentMethodId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await paymentMethods.ListByCustomerAsync(customer.Id, cancellationToken).ConfigureAwait(false);
        foreach (var other in existing.Where(m => m.IsDefault && m.Id != method.Id))
        {
            other.ClearDefault(now);
        }

        method.MakeDefault(now);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
