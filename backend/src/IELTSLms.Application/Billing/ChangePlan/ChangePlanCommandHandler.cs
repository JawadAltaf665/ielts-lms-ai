using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Email;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Billing.GetSubscription;
using IELTSLms.Domain.Billing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing.ChangePlan;

public sealed class ChangePlanCommandHandler(
    ISubscriptionRepository subscriptions, IPlanRepository plans, IPriceRepository prices,
    IBillingCustomerRepository billingCustomers, IPaymentGateway paymentGateway, IEmailSender emailSender,
    IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ChangePlanCommand, SubscriptionResponse>
{
    public async Task<Result<SubscriptionResponse>> Handle(ChangePlanCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetCurrentByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (subscription is null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.SubscriptionNotFound);
        }

        var oldPlan = await plans.GetByIdAsync(subscription.PlanId, cancellationToken).ConfigureAwait(false);

        var newPlan = await plans.GetByIdAsync(PlanId.From(request.NewPlanId), cancellationToken).ConfigureAwait(false);
        if (newPlan is null || !newPlan.IsActive)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PlanNotFound);
        }

        var newPrice = await prices.GetByIdAsync(PriceId.From(request.NewPriceId), cancellationToken).ConfigureAwait(false);
        if (newPrice is null || !newPrice.IsActive)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PriceNotFound);
        }

        if (newPrice.PlanId != newPlan.Id)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PriceDoesNotBelongToPlan);
        }

        if (newPrice.StripePriceId is null)
        {
            return Result.Failure<SubscriptionResponse>(BillingErrors.PriceInactive);
        }

        var now = clock.UtcNow;
        var changeResult = subscription.ChangePlan(newPlan.Id, newPrice.Id, now);
        if (changeResult.IsFailure)
        {
            return Result.Failure<SubscriptionResponse>(changeResult.Error);
        }

        if (subscription.StripeSubscriptionId is not null)
        {
            await paymentGateway
                .ChangeSubscriptionPriceAsync(subscription.StripeSubscriptionId, newPrice.StripePriceId, cancellationToken)
                .ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var customer = await billingCustomers.GetByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (customer is not null && oldPlan is not null)
        {
            var direction = newPlan.CompareTierTo(oldPlan) > 0 ? "upgraded" : "downgraded";
            await emailSender.SendAsync(
                new EmailMessage(
                    customer.Email,
                    $"Your plan has been {direction}",
                    $"<p>Your subscription has been {direction} to {newPlan.Name}.</p>",
                    $"Your subscription has been {direction} to {newPlan.Name}."),
                cancellationToken).ConfigureAwait(false);
        }

        return Result.Success(GetSubscriptionQueryHandler.ToResponse(subscription));
    }
}
