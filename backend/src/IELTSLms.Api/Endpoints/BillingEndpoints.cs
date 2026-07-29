using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Billing.CancelSubscription;
using IELTSLms.Application.Billing.ChangePlan;
using IELTSLms.Application.Billing.CreateBillingPortalSession;
using IELTSLms.Application.Billing.CreateCheckoutSession;
using IELTSLms.Application.Billing.CreateSetupIntent;
using IELTSLms.Application.Billing.CreateSubscription;
using IELTSLms.Application.Billing.GetBilling;
using IELTSLms.Application.Billing.GetBillingAnalytics;
using IELTSLms.Application.Billing.GetFeatures;
using IELTSLms.Application.Billing.GetFinancialSummary;
using IELTSLms.Application.Billing.GetInvoiceById;
using IELTSLms.Application.Billing.GetInvoices;
using IELTSLms.Application.Billing.GetPayments;
using IELTSLms.Application.Billing.GetPlans;
using IELTSLms.Application.Billing.GetPricing;
using IELTSLms.Application.Billing.GetQuotas;
using IELTSLms.Application.Billing.GetSubscription;
using IELTSLms.Application.Billing.PauseSubscription;
using IELTSLms.Application.Billing.RefundPayment;
using IELTSLms.Application.Billing.RemovePaymentMethod;
using IELTSLms.Application.Billing.ResumeSubscription;
using IELTSLms.Application.Billing.RetryPayment;
using IELTSLms.Application.Billing.SetDefaultPaymentMethod;
using IELTSLms.Application.Billing.UnpauseSubscription;
using IELTSLms.Application.Billing.ValidateCoupon;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Subscription & Billing endpoints (13, ADR-031). Every route here is tenant-scoped and
/// requires authentication — unlike <c>TenantEndpoints</c>' pre-auth provisioning routes, billing
/// always acts on the caller's already-resolved tenant, read from <see cref="ITenantContext"/>.
/// </summary>
internal static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/plans", GetPlansAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetPlans")
            .WithSummary("Lists active subscription plans with their prices.");

        builder.MapGet("/pricing", GetPricingAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetPricing")
            .WithSummary("Lists all active prices across plans.");

        builder.MapGet("/subscription", GetSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetSubscription")
            .WithSummary("Gets the caller's tenant's current subscription.");

        builder.MapPost("/checkout", CreateCheckoutSessionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("CreateCheckoutSession")
            .WithSummary("Starts a Stripe Checkout session for a paid plan.");

        builder.MapPost("/subscription", CreateSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("CreateSubscription")
            .WithSummary("Creates a subscription directly (Free plan only — paid plans use /checkout).");

        builder.MapPost("/subscription/cancel", CancelSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("CancelSubscription")
            .WithSummary("Cancels the caller's tenant's subscription at period end.");

        builder.MapPost("/subscription/resume", ResumeSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("ResumeSubscription")
            .WithSummary("Reverses a pending cancellation before the current period ends.");

        builder.MapGet("/billing", GetBillingAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetBilling")
            .WithSummary("Gets the caller's tenant's billing summary: customer, payment methods, recent invoices.");

        builder.MapGet("/features", GetFeaturesAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetFeatures")
            .WithSummary("Gets the caller's tenant's effective feature entitlements.");

        builder.MapGet("/quotas", GetQuotasAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetQuotas")
            .WithSummary("Gets the caller's tenant's effective quotas and current usage.");

        // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

        builder.MapGet("/invoices", GetInvoicesAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetInvoices")
            .WithSummary("Searches the caller's tenant's invoice history.");

        builder.MapGet("/invoices/{id:guid}", GetInvoiceByIdAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetInvoiceById")
            .WithSummary("Gets a single invoice, including its Stripe-hosted download URL.");

        builder.MapGet("/payments", GetPaymentsAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("GetPayments")
            .WithSummary("Lists the caller's tenant's payment history.");

        builder.MapPost("/payments/{id:guid}/retry", RetryPaymentAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("RetryPayment")
            .WithSummary("Re-attempts a failed payment.");

        builder.MapPost("/payments/{id:guid}/refund", RefundPaymentAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("RefundPayment")
            .WithSummary("Issues a full or partial refund against a succeeded payment.");

        builder.MapPost("/billing-portal", CreateBillingPortalSessionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("CreateBillingPortalSession")
            .WithSummary("Starts a Stripe Billing Portal session — the primary self-service surface.");

        builder.MapPost("/payment-methods/setup-intent", CreateSetupIntentAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("CreateSetupIntent")
            .WithSummary("Starts collecting a new payment method client-side.");

        builder.MapDelete("/payment-methods/{id:guid}", RemovePaymentMethodAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("RemovePaymentMethod")
            .WithSummary("Removes a saved payment method.");

        builder.MapPost("/payment-methods/{id:guid}/default", SetDefaultPaymentMethodAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("SetDefaultPaymentMethod")
            .WithSummary("Marks a payment method as the tenant's default.");

        builder.MapPost("/subscription/change-plan", ChangePlanAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("ChangePlan")
            .WithSummary("Immediately upgrades or downgrades the subscription, with Stripe-computed proration.");

        builder.MapPost("/subscription/pause", PauseSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("PauseSubscription")
            .WithSummary("Voluntarily pauses the active subscription.");

        builder.MapPost("/subscription/unpause", UnpauseSubscriptionAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("UnpauseSubscription")
            .WithSummary("Reverses a voluntary pause.");

        builder.MapGet("/coupons/validate", ValidateCouponAsync)
            .RequireAuthorization()
            .WithTags("Billing")
            .WithName("ValidateCoupon")
            .WithSummary("Checks a coupon code's validity, redemption cap, and tenant eligibility.");

        builder.MapGet("/reports/financial-summary", GetFinancialSummaryAsync)
            .RequireAuthorization()
            .WithTags("Billing Reports")
            .WithName("GetFinancialSummary")
            .WithSummary("Cross-tenant financial report (revenue, MRR/ARR, churn, refunds). Platform admin only.");

        builder.MapGet("/reports/analytics", GetBillingAnalyticsAsync)
            .RequireAuthorization()
            .WithTags("Billing Reports")
            .WithName("GetBillingAnalytics")
            .WithSummary("Cross-tenant operational analytics (active subscriptions, churn rate, LTV). Platform admin only.");

        return builder;
    }

    private static async Task<IResult> GetPlansAsync(ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPlansQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetPricingAsync(ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPricingQuery(), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetSubscriptionAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetSubscriptionQuery(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new CreateCheckoutSessionCommand(
            tenantId, request.Email, request.Name, request.PlanId, request.PriceId, request.SuccessUrl, request.CancelUrl,
            request.CouponCode);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateSubscriptionAsync(
        CreateSubscriptionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new CreateSubscriptionCommand(tenantId, request.Email, request.Name, request.PlanId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/subscription", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CancelSubscriptionAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new CancelSubscriptionCommand(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ResumeSubscriptionAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new ResumeSubscriptionCommand(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetBillingAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetBillingQuery(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetFeaturesAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetFeaturesQuery(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetQuotasAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetQuotasQuery(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

    private static async Task<IResult> GetInvoicesAsync(
        [AsParameters] GetInvoicesQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var query = new GetInvoicesQuery(tenantId, queryParams.Status, queryParams.Page ?? 1, queryParams.PageSize ?? 20);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetInvoiceByIdAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetInvoiceByIdQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetPaymentsAsync(
        [AsParameters] GetPaymentsQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var query = new GetPaymentsQuery(tenantId, queryParams.Page ?? 1, queryParams.PageSize ?? 20);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RetryPaymentAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new RetryPaymentCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RefundPaymentAsync(
        Guid id, RefundPaymentRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new RefundPaymentCommand(tenantId, id, request.AmountMinorUnits, request.Reason);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateBillingPortalSessionAsync(
        CreateBillingPortalSessionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new CreateBillingPortalSessionCommand(tenantId, request.ReturnUrl), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateSetupIntentAsync(
        CreateSetupIntentRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new CreateSetupIntentCommand(tenantId, request.Email, request.Name);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RemovePaymentMethodAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new RemovePaymentMethodCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SetDefaultPaymentMethodAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new SetDefaultPaymentMethodCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ChangePlanAsync(
        ChangePlanRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new ChangePlanCommand(tenantId, request.NewPlanId, request.NewPriceId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> PauseSubscriptionAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new PauseSubscriptionCommand(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> UnpauseSubscriptionAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new UnpauseSubscriptionCommand(tenantId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.Empty(httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ValidateCouponAsync(
        string code, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new ValidateCouponQuery(tenantId, code), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetFinancialSummaryAsync(
        [AsParameters] GetReportsQueryParams queryParams, ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveReportWindow(queryParams);
        var result = await sender.Send(new GetFinancialSummaryQuery(from, to), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetBillingAnalyticsAsync(
        [AsParameters] GetReportsQueryParams queryParams, ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveReportWindow(queryParams);
        var result = await sender.Send(new GetBillingAnalyticsQuery(from, to), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static (DateTimeOffset From, DateTimeOffset To) ResolveReportWindow(GetReportsQueryParams queryParams)
    {
        var to = queryParams.To ?? DateTimeOffset.UtcNow;
        var from = queryParams.From ?? to.AddMonths(-1);
        return (from, to);
    }

    private static bool TryGetTenantId(ITenantContext tenantContext, out Guid tenantId)
    {
        if (tenantContext.TenantId is { } id)
        {
            tenantId = id;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }

    private static readonly Error TenantNotResolved = Error.Failure(
        "AUTHZ_TENANT_NOT_RESOLVED", "No tenant could be resolved for this request.");
}
