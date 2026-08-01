using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.GetBandPrediction;
using IELTSLms.Application.AI.RequestBandPrediction;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the standalone, history-based Band Prediction Workflow (Phase 6 Sprint 9, Agentic AI
/// Architecture §13.6). Mirrors <see cref="WritingEndpoints"/>'s async-kickoff shape: <c>POST</c>
/// returns <c>202</c> + an operation resource pollable at <c>GET /operations/{id}</c>, plus this
/// module's own <c>GET /band-predictions/{id}</c> for the prediction result itself.
/// </summary>
internal static class BandPredictionEndpoints
{
    public static IEndpointRouteBuilder MapBandPredictionEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/users/{userId:guid}/band-predictions", RequestBandPredictionAsync)
            .RequireAuthorization()
            .WithTags("BandPrediction")
            .WithName("RequestBandPrediction")
            .WithSummary(
                "Requests a fresh history-based band prediction (band-prediction.request). " +
                "Returns 202 + an operation resource pollable at GET /operations/{id}.");

        builder.MapGet("/band-predictions/{id:guid}", GetBandPredictionAsync)
            .RequireAuthorization()
            .WithTags("BandPrediction")
            .WithName("GetBandPrediction")
            .WithSummary("Gets a band prediction result (band-prediction.request own / analytics.view.cohort).");

        return builder;
    }

    private static async Task<IResult> RequestBandPredictionAsync(
        Guid userId, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var callerUserId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        if (userId != callerUserId && !httpContext.User.IsInRole("Teacher"))
        {
            return Forbidden.ToProblem(httpContext);
        }

        var result = await sender.Send(new RequestBandPredictionCommand(tenantId, userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem(httpContext);
        }

        return Results.Accepted($"/api/v1/band-predictions/{result.Value.BandPredictionId}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetBandPredictionAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetBandPredictionQuery(tenantId, userId, id), cancellationToken);
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

    private static bool TryGetUserId(HttpContext httpContext, out Guid userId) =>
        Guid.TryParse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);

    private static readonly Error TenantNotResolved = Error.Failure(
        "AUTHZ_TENANT_NOT_RESOLVED", "No tenant could be resolved for this request.");

    private static readonly Error UserNotResolved = Error.Failure(
        "AUTHZ_USER_NOT_RESOLVED", "No user could be resolved for this request.");

    private static readonly Error Forbidden = Error.Forbidden(
        "AUTHZ_FORBIDDEN", "You do not have permission to request a band prediction for this learner.");
}
