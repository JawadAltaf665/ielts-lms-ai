using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.GetSubscription;

public sealed class GetSubscriptionQueryHandler(ISubscriptionRepository subscriptions)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionResponse>
{
    public async Task<Result<SubscriptionResponse>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.SubscriptionNotFound);
        }

        return Result.Success(ToResponse(subscription));
    }

    internal static SubscriptionResponse ToResponse(Subscription subscription) => new(
        subscription.Id.Value,
        subscription.PlanId.Value,
        subscription.PriceId.Value,
        subscription.Status.ToString(),
        subscription.CurrentPeriodStart,
        subscription.CurrentPeriodEnd,
        subscription.TrialEndsAtUtc,
        subscription.CancelAtPeriodEnd);
}
