using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler(
    ISubscriptionRepository subscriptions, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotFound);
        }

        var cancelResult = subscription.Cancel(clock.UtcNow);
        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        if (subscription.StripeSubscriptionId is not null)
        {
            await paymentGateway
                .CancelSubscriptionAsync(subscription.StripeSubscriptionId, atPeriodEnd: true, cancellationToken)
                .ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
