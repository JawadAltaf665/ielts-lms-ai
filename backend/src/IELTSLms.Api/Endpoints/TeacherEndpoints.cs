using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.Teacher.Commands.RespondToLessonStep;
using IELTSLms.Application.AI.Teacher.Commands.StartLessonSession;
using IELTSLms.Application.AI.Teacher.Queries.GetLessonSession;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>Maps AI Teacher's structured lesson-delivery endpoints (Phase 7 Sprint 4): start a session
/// for a lesson, respond to whichever step is current, and read session state/history.</summary>
internal static class TeacherEndpoints
{
    public static IEndpointRouteBuilder MapTeacherEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/ai/teacher/lessons/{lessonId:guid}/sessions", StartLessonSessionAsync)
            .RequireAuthorization()
            .WithTags("AI Teacher")
            .WithName("StartLessonSession")
            .WithSummary(
                "Starts a structured AI Teacher session for a lesson (ai.teacher.use). Returns 202 + an operation " +
                "resource for the Explain step's opening content.");

        builder.MapPost("/ai/teacher/sessions/{id:guid}/respond", RespondToLessonStepAsync)
            .RequireAuthorization()
            .WithTags("AI Teacher")
            .WithName("RespondToLessonStep")
            .WithSummary("Responds to the session's current step (ai.teacher.use). Returns 202 + an operation resource.");

        builder.MapGet("/ai/teacher/sessions/{id:guid}", GetLessonSessionAsync)
            .RequireAuthorization()
            .WithTags("AI Teacher")
            .WithName("GetLessonSession")
            .WithSummary("Gets a lesson session's current step and conversation history (ai.teacher.use).");

        return builder;
    }

    private static async Task<IResult> StartLessonSessionAsync(
        Guid lessonId, StartLessonSessionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var command = new StartLessonSessionCommand(tenantId, userId, lessonId, request.Topic);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.OperationId}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> RespondToLessonStepAsync(
        Guid id, RespondToLessonStepRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var command = new RespondToLessonStepCommand(tenantId, userId, id, request.Message);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetLessonSessionAsync(
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

        var result = await sender.Send(new GetLessonSessionQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    /// <summary>Sets <c>Retry-After</c> on a quota-exceeded response (API Design Standards §9.2), same as <c>AiEndpoints</c>.</summary>
    private static IResult ToProblemWithRetryAfter(Error error, HttpContext httpContext)
    {
        if (error.Type == ErrorType.RateLimited)
        {
            var now = DateTimeOffset.UtcNow;
            var nextMidnightUtc = now.Date.AddDays(1);
            var retryAfterSeconds = (int)Math.Ceiling((nextMidnightUtc - now).TotalSeconds);
            httpContext.Response.Headers["Retry-After"] = retryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return error.ToProblem(httpContext);
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
}

public sealed record StartLessonSessionRequest(string Topic);

public sealed record RespondToLessonStepRequest(string Message);
