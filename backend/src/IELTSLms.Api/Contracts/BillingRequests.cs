namespace IELTSLms.Api.Contracts;

public sealed record CreateCheckoutSessionRequest(
    string Email, string? Name, Guid PlanId, Guid PriceId, string SuccessUrl, string CancelUrl, string? CouponCode = null);

public sealed record CreateSubscriptionRequest(string Email, string? Name, Guid PlanId);

// ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

public sealed record CreateBillingPortalSessionRequest(string ReturnUrl);

public sealed record CreateSetupIntentRequest(string Email, string? Name);

public sealed record ChangePlanRequest(Guid NewPlanId, Guid NewPriceId);

public sealed record RefundPaymentRequest(long AmountMinorUnits, string Reason);

public sealed record GetInvoicesQueryParams(string? Status, int? Page, int? PageSize);

public sealed record GetPaymentsQueryParams(int? Page, int? PageSize);

public sealed record GetReportsQueryParams(DateTimeOffset? From, DateTimeOffset? To);
