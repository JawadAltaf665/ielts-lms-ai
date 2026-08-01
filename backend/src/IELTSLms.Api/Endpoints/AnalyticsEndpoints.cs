using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Analytics.Queries.GetAttemptReview;
using IELTSLms.Application.Analytics.Queries.GetExamHistory;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Sprint 9 Results/History endpoints (Phase 6 Sprint 9): cross-skill exam history and
/// per-attempt review. Both are read-only integrations over Sprints 1-8's own tables — no new write path.
/// </summary>
internal static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/users/{userId:guid}/exam-history", GetExamHistoryAsync)
            .RequireAuthorization()
            .WithTags("Analytics")
            .WithName("GetExamHistory")
            .WithSummary("Cross-skill attempt/submission history for a learner, cursor-paginated (analytics.view.self / analytics.view.cohort).");

        builder.MapGet("/attempts/{type}/{id:guid}/review", GetAttemptReviewAsync)
            .RequireAuthorization()
            .WithTags("Analytics")
            .WithName("GetAttemptReview")
            .WithSummary(
                "Pulls one past attempt + its AI feedback for review. {type} is writing|speaking|reading|" +
                "listening|grammar|vocabulary|mock-exam.");

        return builder;
    }

    private static async Task<IResult> GetExamHistoryAsync(
        Guid userId, [AsParameters] GetExamHistoryQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
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

        var limit = PaginationDefaults.NormalizePageSize(queryParams.Limit ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(new GetExamHistoryQuery(tenantId, userId, queryParams.Cursor, limit), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetAttemptReviewAsync(
        string type, Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetAttemptReviewQuery(tenantId, userId, type, id), cancellationToken);
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
        "AUTHZ_FORBIDDEN", "You do not have permission to view this learner's history.");
}
