using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.RemovePaymentMethod;

public sealed class RemovePaymentMethodCommandHandler(
    IPaymentMethodRepository paymentMethods, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork)
    : ICommandHandler<RemovePaymentMethodCommand>
{
    public async Task<Result> Handle(RemovePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var method = await paymentMethods.GetByIdAsync(PaymentMethodId.From(request.PaymentMethodId), cancellationToken)
            .ConfigureAwait(false);
        if (method is null || method.TenantId != request.TenantId)
        {
            return Result.Failure(BillingErrors.PaymentMethodNotFound);
        }

        await paymentGateway.DetachPaymentMethodAsync(method.StripePaymentMethodId, cancellationToken).ConfigureAwait(false);
        paymentMethods.Remove(method);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
