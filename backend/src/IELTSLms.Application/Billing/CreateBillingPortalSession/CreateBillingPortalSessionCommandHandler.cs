using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.CreateBillingPortalSession;

public sealed class CreateBillingPortalSessionCommandHandler(
    IBillingCustomerRepository billingCustomers, IPaymentGateway paymentGateway)
    : ICommandHandler<CreateBillingPortalSessionCommand, BillingPortalSessionResponse>
{
    public async Task<Result<BillingPortalSessionResponse>> Handle(
        CreateBillingPortalSessionCommand request, CancellationToken cancellationToken)
    {
        var customer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (customer?.StripeCustomerId is null)
        {
            return Result.Failure<BillingPortalSessionResponse>(BillingErrors.CustomerNotFound);
        }

        var url = await paymentGateway
            .CreateBillingPortalSessionAsync(customer.StripeCustomerId, request.ReturnUrl, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new BillingPortalSessionResponse(url));
    }
}
