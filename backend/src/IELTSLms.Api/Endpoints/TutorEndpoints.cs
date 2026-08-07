using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.Tutor.Commands.AskTutor;
using IELTSLms.Application.AI.Tutor.Commands.StartTutorConversation;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps AI Tutor's reactive Q&amp;A endpoints (Phase 7 Sprint 4, <c>FR-AITUTOR-001</c>). The
/// conversation resource itself is read back through <see cref="ConversationEndpoints"/> — this file
/// only owns the two Tutor-specific write actions (start, ask).
/// </summary>
internal static class TutorEndpoints
{
    public static IEndpointRouteBuilder MapTutorEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/ai/tutor/conversations", StartTutorConversationAsync)
            .RequireAuthorization()
            .WithTags("AI Tutor")
            .WithName("StartTutorConversation")
            .WithSummary("Starts a new AI Tutor conversation (ai.tutor.use).");

        builder.MapPost("/ai/tutor/conversations/{id:guid}/messages", AskTutorAsync)
            .RequireAuthorization()
            .WithTags("AI Tutor")
            .WithName("AskTutor")
            .WithSummary("Asks the Tutor a question within an existing conversation (ai.tutor.use). Returns 202 + an operation resource.");

        return builder;
    }

    private static async Task<IResult> StartTutorConversationAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new StartTutorConversationCommand(tenantId, userId), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Created($"/api/v1/conversations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> AskTutorAsync(
        Guid id, AskTutorRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new AskTutorCommand(tenantId, userId, id, request.Question);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
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

public sealed record AskTutorRequest(string Question);
