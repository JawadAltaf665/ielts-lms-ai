using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Billing.CancelSubscription;
using IELTSLms.Application.Billing.CreateCheckoutSession;
using IELTSLms.Application.Billing.CreateSubscription;
using IELTSLms.Application.Billing.GetBilling;
using IELTSLms.Application.Billing.GetFeatures;
using IELTSLms.Application.Billing.GetPlans;
using IELTSLms.Application.Billing.GetPricing;
using IELTSLms.Application.Billing.GetQuotas;
using IELTSLms.Application.Billing.GetSubscription;
using IELTSLms.Application.Billing.ResumeSubscription;
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
            tenantId, request.Email, request.Name, request.PlanId, request.PriceId, request.SuccessUrl, request.CancelUrl);
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
