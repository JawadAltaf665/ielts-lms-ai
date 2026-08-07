using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Learning.Recommendations.Commands.DismissRecommendation;
using IELTSLms.Application.Learning.Recommendations.Queries.GetRecommendations;
using IELTSLms.Application.Learning.StudyPlanner.Commands.CompleteDailyPlanItem;
using IELTSLms.Application.Learning.StudyPlanner.Queries.GetDailyPlan;
using IELTSLms.Application.Learning.WeaknessDetection.Commands.RefreshWeaknessProfile;
using IELTSLms.Application.Learning.WeaknessDetection.Queries.GetWeaknessProfile;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Sprint 9 Study Planner / Weakness Detection / Learning Recommendations endpoints (sprint
/// API table). Self-or-cohort access follows <see cref="AnalyticsEndpoints"/>'s established shape
/// (inline self/Teacher check, not a separate permission-string pipeline) — <c>analytics.view.self</c> /
/// <c>analytics.view.cohort</c> in the sprint table map to that same check. The dismiss endpoint is not
/// in the sprint's literal endpoint table; it exists because the business rules and required Tier-1
/// test explicitly call for a dismiss lifecycle (self-only action sub-resource, API Design Standards §4).
/// </summary>
internal static class LearningEndpoints
{
    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/{userId:guid}/weakness-profile", GetWeaknessProfileAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("GetWeaknessProfile")
            .WithSummary("Gets the learner's current weakness profile (analytics.view.self / analytics.view.cohort).");

        builder.MapPost("/users/{userId:guid}/weakness-profile/refresh", RefreshWeaknessProfileAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("RefreshWeaknessProfile")
            .WithSummary(
                "Force-recomputes the weakness profile (analytics.view.self, learner-triggered, rate-limited). " +
                "Returns 202 + an operation resource pollable at GET /operations/{id}.");

        builder.MapGet("/users/{userId:guid}/recommendations", GetRecommendationsAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("GetRecommendations")
            .WithSummary("Gets the learner's current ranked recommendations (analytics.view.self / analytics.view.cohort).");

        builder.MapPost("/users/{userId:guid}/recommendations/{id:guid}/dismiss", DismissRecommendationAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("DismissRecommendation")
            .WithSummary("Dismisses the learner's active recommendation set (analytics.view.self, self-only).");

        builder.MapGet("/users/{userId:guid}/daily-plans/today", GetDailyPlanTodayAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("GetDailyPlanToday")
            .WithSummary(
                "Gets today's adaptive plan (analytics.view.self / analytics.view.cohort); generates it on " +
                "first request of the day if the scheduled sweep hasn't run yet.");

        builder.MapPost("/users/{userId:guid}/daily-plans/today/items/{itemId:guid}/complete", CompleteDailyPlanItemAsync)
            .RequireAuthorization()
            .WithTags("Learning")
            .WithName("CompleteDailyPlanItem")
            .WithSummary("Marks a plan item complete (analytics.view.self, self-only); feeds ProgressTracking.");

        return builder;
    }

    private static async Task<IResult> GetWeaknessProfileAsync(
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

        var result = await sender.Send(new GetWeaknessProfileQuery(tenantId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RefreshWeaknessProfileAsync(
        Guid userId, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var callerUserId) || userId != callerUserId)
        {
            return Forbidden.ToProblem(httpContext);
        }

        var result = await sender.Send(new RefreshWeaknessProfileCommand(tenantId, userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem(httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetRecommendationsAsync(
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

        var result = await sender.Send(new GetRecommendationsQuery(tenantId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> DismissRecommendationAsync(
        Guid userId, Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var callerUserId) || userId != callerUserId)
        {
            return Forbidden.ToProblem(httpContext);
        }

        var result = await sender.Send(new DismissRecommendationCommand(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetDailyPlanTodayAsync(
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

        var result = await sender.Send(new GetDailyPlanQuery(tenantId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CompleteDailyPlanItemAsync(
        Guid userId, Guid itemId, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var callerUserId) || userId != callerUserId)
        {
            return Forbidden.ToProblem(httpContext);
        }

        var result = await sender.Send(new CompleteDailyPlanItemCommand(tenantId, userId, itemId), cancellationToken);
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
        "AUTHZ_FORBIDDEN", "You do not have permission to access this learner's data.");
}
