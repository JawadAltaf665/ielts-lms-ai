using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.CreateSetupIntent;

public sealed class CreateSetupIntentCommandHandler(
    IBillingCustomerRepository billingCustomers, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateSetupIntentCommand, SetupIntentResponse>
{
    public async Task<Result<SetupIntentResponse>> Handle(CreateSetupIntentCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var customer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (customer is null)
        {
            var createResult = BillingCustomer.Create(
                BillingCustomerId.From(guids.NewGuid()), request.TenantId, request.Email, request.Name, now);
            if (createResult.IsFailure)
            {
                return Result.Failure<SetupIntentResponse>(createResult.Error);
            }

            customer = createResult.Value;
            billingCustomers.Add(customer);
        }

        var stripeCustomerId = await paymentGateway
            .EnsureCustomerAsync(customer.StripeCustomerId, request.Email, request.Name, cancellationToken)
            .ConfigureAwait(false);
        customer.AttachStripeCustomer(stripeCustomerId, now);

        var clientSecret = await paymentGateway.CreateSetupIntentAsync(stripeCustomerId, cancellationToken).ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(new SetupIntentResponse(clientSecret));
    }
}
