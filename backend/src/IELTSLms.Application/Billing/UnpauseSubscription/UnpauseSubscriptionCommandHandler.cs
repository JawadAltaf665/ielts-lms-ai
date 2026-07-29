using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.UnpauseSubscription;

public sealed class UnpauseSubscriptionCommandHandler(ISubscriptionRepository subscriptions, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UnpauseSubscriptionCommand>
{
    public async Task<Result> Handle(UnpauseSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure(BillingErrors.SubscriptionNotFound);
        }

        var result = subscription.Unpause(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
