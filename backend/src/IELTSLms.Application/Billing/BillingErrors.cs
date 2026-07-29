using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Billing;

public static class BillingErrors
{
    public static readonly Error PlanNotFound = Error.NotFound(
        "BILLING_PLAN_NOT_FOUND", "The requested plan does not exist.");

    public static readonly Error PlanInactive = Error.Conflict(
        "BILLING_PLAN_INACTIVE", "The requested plan is not currently available.");

    public static readonly Error PriceNotFound = Error.NotFound(
        "BILLING_PRICE_NOT_FOUND", "The requested price does not exist.");

    public static readonly Error PriceInactive = Error.Conflict(
        "BILLING_PRICE_INACTIVE", "The requested price is not currently available.");

    public static readonly Error PriceDoesNotBelongToPlan = Error.Validation(
        "BILLING_PRICE_PLAN_MISMATCH", "The price does not belong to the requested plan.");

    public static readonly Error SubscriptionNotFound = Error.NotFound(
        "BILLING_SUBSCRIPTION_NOT_FOUND", "The tenant has no subscription.");

    public static readonly Error SubscriptionAlreadyExists = Error.Conflict(
        "BILLING_SUBSCRIPTION_ALREADY_EXISTS", "The tenant already has a subscription. Use checkout to change plans.");

    public static readonly Error InvalidTransition = Error.Conflict(
        "BILLING_SUBSCRIPTION_INVALID_TRANSITION", "The subscription cannot perform this operation in its current state.");

    public static readonly Error FreePlanRequiresNoCheckout = Error.Validation(
        "BILLING_FREE_PLAN_DIRECT_ONLY", "Only the Free plan can be created directly; paid plans require checkout.");

    public static readonly Error WebhookEventAlreadyProcessed = Error.Conflict(
        "BILLING_WEBHOOK_ALREADY_PROCESSED", "This webhook event has already been processed.");

    public static readonly Error WebhookSignatureInvalid = Error.Validation(
        "BILLING_WEBHOOK_SIGNATURE_INVALID", "The webhook signature could not be verified.");

    public static readonly Error QuotaExceeded = Error.Custom(
        "BUSINESS_QUOTA_EXCEEDED", "The requested usage exceeds the tenant's remaining quota.", ErrorType.Forbidden);

    public static readonly Error SubscriptionRequired = Error.Custom(
        "BUSINESS_SUBSCRIPTION_REQUIRED", "An active subscription is required for this action.", ErrorType.Forbidden);
}
