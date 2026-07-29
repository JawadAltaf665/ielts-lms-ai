using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.PauseSubscription;

public sealed class PauseSubscriptionCommandHandler(ISubscriptionRepository subscriptions, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<PauseSubscriptionCommand>
{
    public async Task<Result> Handle(PauseSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotFound);
        }

        var result = subscription.Pause(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
