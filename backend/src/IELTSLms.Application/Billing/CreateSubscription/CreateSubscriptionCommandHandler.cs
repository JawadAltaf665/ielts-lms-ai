using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetSubscription;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler(
    IPlanRepository plans,
    IPriceRepository prices,
    IBillingCustomerRepository billingCustomers,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids)
    : ICommandHandler<CreateSubscriptionCommand, SubscriptionResponse>
{
    public async Task<Result<SubscriptionResponse>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await plans.GetByIdAsync(PlanId.From(request.PlanId), cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PlanNotFound);
        }

        if (!plan.IsActive)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PlanInactive);
        }

        if (plan.Code is not PlanCode.Free)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.FreePlanRequiresNoCheckout);
        }

        var existingSubscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (existingSubscription is not null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.SubscriptionAlreadyExists);
        }

        var planPrices = await prices.ListByPlanAsync(plan.Id, cancellationToken).ConfigureAwait(false);
        var freePrice = planPrices.FirstOrDefault(p => p.IsActive);
        if (freePrice is null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PriceNotFound);
        }

        var now = clock.UtcNow;

        var billingCustomer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (billingCustomer is null)
        {
            var createResult = BillingCustomer.Create(
                BillingCustomerId.From(guids.NewGuid()), request.TenantId, request.Email, request.Name, now);
            if (createResult.IsFailure)
            {
                return Result.Failure<SubscriptionResponse>(createResult.Error);
            }

            billingCustomer = createResult.Value;
            billingCustomers.Add(billingCustomer);
        }

        var subscription = Subscription.Create(
            SubscriptionId.From(guids.NewGuid()), request.TenantId, billingCustomer.Id, plan.Id, freePrice.Id, now);

        var activateResult = subscription.Activate(now, now.AddYears(100), stripeSubscriptionId: null, now);
        if (activateResult.IsFailure)
        {
            return Result.Failure<SubscriptionResponse>(activateResult.Error);
        }

        subscriptions.Add(subscription);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(GetSubscriptionQueryHandler.ToResponse(subscription));
    }
}
