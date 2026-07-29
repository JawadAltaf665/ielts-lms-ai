using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.ResumeSubscription;

public sealed class ResumeSubscriptionCommandHandler(
    ISubscriptionRepository subscriptions, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ResumeSubscriptionCommand>
{
    public async Task<Result> Handle(ResumeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotFound);
        }

        var resumeResult = subscription.Resume(clock.UtcNow);
        if (resumeResult.IsFailure)
        {
            return resumeResult;
        }

        if (subscription.StripeSubscriptionId is not null)
        {
            await paymentGateway.ResumeSubscriptionAsync(subscription.StripeSubscriptionId, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
