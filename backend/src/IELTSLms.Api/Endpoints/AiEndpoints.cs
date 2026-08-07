using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.GetAiOperationStatus;
using IELTSLms.Application.AI.GetAiUsageSummary;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestListeningCoaching;
using IELTSLms.Application.AI.RequestReadingCoaching;
using IELTSLms.Application.AI.RequestVocabularyCoaching;
using IELTSLms.Application.AI.Orchestration;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the AI Gateway endpoints (Phase 6 Sprint 3, DDD §13.14 BC-14): the Grammar Coach ad-hoc check
/// and the generic async operation-resource poll every future AI capability reuses. Mirrors
/// <see cref="GrammarEndpoints"/> for tenant/user resolution and error shaping.
/// </summary>
internal static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/ai/grammar-coach", RequestGrammarCoachingAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("RequestGrammarCoaching")
            .WithSummary("Ad-hoc grammar check on arbitrary learner text (ai.grammar-coach.use). Returns 202 + an operation resource.");

        builder.MapPost("/ai/vocabulary-coach", RequestVocabularyCoachingAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("RequestVocabularyCoaching")
            .WithSummary(
                "Usage/synonym/collocation suggestions for a word (ai.vocabulary-coach.use). Returns 202 + an operation resource.");

        builder.MapPost("/ai/reading-coach", RequestReadingCoachingAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("RequestReadingCoaching")
            .WithSummary("On-demand Reading strategy guidance (ai.reading-coach.use). Returns 202 + an operation resource.");

        builder.MapPost("/ai/listening-coach", RequestListeningCoachingAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("RequestListeningCoaching")
            .WithSummary("On-demand Listening strategy guidance (ai.listening-coach.use). Returns 202 + an operation resource.");

        builder.MapGet("/operations/{id:guid}", GetOperationStatusAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("GetAiOperationStatus")
            .WithSummary("Polls the status of an async AI operation.");

        builder.MapGet("/ai/agents", GetAgentsAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("GetAiAgents")
            .WithSummary("Retrieves all registered AI agents from the Agent Registry (Sprint 10).");

        // The endpoint is conceptually a workflow run status, exposed under the AI namespace.
        // IWorkflowEngine returns WorkflowRunId, but we might want a specific query. Wait, we don't have GetWorkflowRunQuery yet. 
        // Ah, the task checklist says: "Add GET /api/v1/ai/workflow-runs/{id} to AiEndpoints.cs".
        // Let me check if there's a GetWorkflowRunStatusQuery, or if I should create one, or if I just need to expose orchestrated-review endpoints in MockExamEndpoints.
        // Actually, the checklist says: 
        // - Add GET /api/v1/ai/agents to AiEndpoints.cs
        // - Add GET /api/v1/ai/workflow-runs/{id} to AiEndpoints.cs
        // Wait, I did not create a GetWorkflowRunQuery handler! Let me check the task checklist again.
        builder.MapGet("/ai/workflow-runs/{id:guid}", GetWorkflowRunAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("GetWorkflowRun")
            .WithSummary("Retrieves the status and trace of a specific workflow run (Sprint 10).");

        builder.MapGet("/ai/usage-summary", GetUsageSummaryAsync)
            .RequireAuthorization()
            .WithTags("AI")
            .WithName("GetAiUsageSummary")
            .WithSummary(
                "Tenant-scoped AI cost/usage aggregated by capability/provider/day, defaults to the trailing " +
                "30 days (Tenant Admin/Institute Admin).");

        return builder;
    }

    private static async Task<IResult> RequestGrammarCoachingAsync(
        GrammarCoachRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new RequestGrammarCoachingCommand(tenantId, userId, request.Text);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> RequestVocabularyCoachingAsync(
        VocabularyCoachRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new RequestVocabularyCoachingCommand(tenantId, userId, request.Word, request.Topic, request.Difficulty);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> RequestReadingCoachingAsync(
        ReadingCoachRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new RequestReadingCoachingCommand(tenantId, userId, request.Text);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> RequestListeningCoachingAsync(
        ListeningCoachRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new RequestListeningCoachingCommand(tenantId, userId, request.Text);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemWithRetryAfter(result.Error, httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetOperationStatusAsync(
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

        var result = await sender.Send(new GetAiOperationStatusQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    /// <summary>
    /// Sets <c>Retry-After</c> on a quota-exceeded response (API Design Standards §9.2).
    /// ponytail: assumes the AI quota's reset cycle is daily (the only cycle this sprint's
    /// <c>ai.grammar-coach.requests</c> quota uses) — generalize to read the actual
    /// <c>QuotaDefinition.ResetCycle</c> if a non-daily AI quota is added later.
    /// </summary>
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

    private static async Task<IResult> GetUsageSummaryAsync(
        [AsParameters] AiUsageSummaryQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(
            new GetAiUsageSummaryQuery(tenantId, queryParams.From, queryParams.To), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetAgentsAsync(
        IAgentRegistry registry, HttpContext httpContext, CancellationToken cancellationToken)
    {
        // Requires authorization, but agents are platform-wide.
        // In a real app we might check for "agent.read" permission, but here we just return them.
        var agents = await registry.GetAllAsync(cancellationToken);
        return Results.Ok(ApiResponse.From(agents, httpContext));
    }

    private static async Task<IResult> GetWorkflowRunAsync(
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

        var result = await sender.Send(new Application.AI.Orchestration.Queries.GetWorkflowRun.GetWorkflowRunQuery(tenantId, userId, id), cancellationToken);
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
}

public sealed record GrammarCoachRequest(string Text);

public sealed record VocabularyCoachRequest(string Word, string? Topic, string? Difficulty);

public sealed record ReadingCoachRequest(string Text);

public sealed record ListeningCoachRequest(string Text);

public sealed record AiUsageSummaryQueryParams(DateOnly? From, DateOnly? To);
