namespace IELTSLms.Api.Contracts;

public sealed record CreateCheckoutSessionRequest(
    string Email, string? Name, Guid PlanId, Guid PriceId, string SuccessUrl, string CancelUrl);

public sealed record CreateSubscriptionRequest(string Email, string? Name, Guid PlanId);
