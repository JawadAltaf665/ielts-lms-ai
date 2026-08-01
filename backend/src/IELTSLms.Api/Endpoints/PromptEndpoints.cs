using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.Prompts.ActivatePromptVersion;
using IELTSLms.Application.AI.Prompts.CreatePromptVersion;
using IELTSLms.Application.AI.Prompts.GetPromptTemplate;
using IELTSLms.Application.AI.Prompts.ListPromptTemplates;
using IELTSLms.Application.AI.Prompts.RetirePromptVersion;
using IELTSLms.Application.AI.Prompts.RollbackPromptVersion;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Prompt Library &amp; Versioning admin endpoints (Phase 7 Sprint 2, AI Architecture §5): CRUD,
/// activation (rollout/A-B slot assignment), rollback, and retirement of <c>AiPrompt</c> versions.
/// Entitlement-independent (an operational tool, not a learner-facing capability) — gated by role only,
/// mirroring <see cref="AiEndpoints"/> for tenant/user resolution and error shaping.
/// </summary>
internal static class PromptEndpoints
{
    public static IEndpointRouteBuilder MapPromptEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/ai/prompts", ListPromptTemplatesAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("ListPromptTemplates")
            .WithSummary("Lists prompt templates for the caller's tenant, filterable by feature/scope/status (ai.prompt.manage).");

        builder.MapGet("/ai/prompts/{id:guid}", GetPromptTemplateAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("GetPromptTemplate")
            .WithSummary("Gets one prompt template's full body and metadata (ai.prompt.manage).");

        builder.MapPost("/ai/prompts", CreatePromptVersionAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("CreatePromptVersion")
            .WithSummary("Authors a new prompt version for a feature/scope; never mutates a used version (ai.prompt.manage).");

        builder.MapPatch("/ai/prompts/{id:guid}/activate", ActivatePromptVersionAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("ActivatePromptVersion")
            .WithSummary("Activates a prompt version for its feature/scope (rollout / A-B slot assignment). Requires If-Match (ai.prompt.manage).");

        builder.MapPost("/ai/prompts/{id:guid}/rollback", RollbackPromptVersionAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("RollbackPromptVersion")
            .WithSummary("Rolls a feature/scope back to the version active immediately before the current one (ai.prompt.manage).");

        builder.MapDelete("/ai/prompts/{id:guid}", RetirePromptVersionAsync)
            .RequireAuthorization()
            .WithTags("AI Prompts")
            .WithName("RetirePromptVersion")
            .WithSummary("Retires (soft-deletes) a prompt version. Rejected while active (ai.prompt.manage).");

        return builder;
    }

    private static async Task<IResult> ListPromptTemplatesAsync(
        [AsParameters] ListPromptTemplatesQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var query = new ListPromptTemplatesQuery(
            tenantId, queryParams.Feature, queryParams.Scope, queryParams.IsActive,
            queryParams.Page ?? 1, queryParams.PageSize ?? 20);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetPromptTemplateAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetPromptTemplateQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreatePromptVersionAsync(
        CreatePromptVersionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var command = new CreatePromptVersionCommand(tenantId, request.Feature, request.Scope, request.SystemTemplate, request.AiProviderId);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Created($"/api/v1/ai/prompts/{result.Value.Id}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> ActivatePromptVersionAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var ifMatch = TryParseIfMatch(httpContext);
        var result = await sender.Send(new ActivatePromptVersionCommand(tenantId, id, ifMatch), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RollbackPromptVersionAsync(
        Guid id, RollbackPromptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var result = await sender.Send(new RollbackPromptVersionCommand(tenantId, id, userId, request.Reason), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RetirePromptVersionAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new RetirePromptVersionCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
    }

    /// <summary>
    /// Parses the client's last-known <c>RowVersion</c> from <c>If-Match</c> (bare or quoted integer;
    /// no generic ETag middleware exists in this codebase yet — see <c>ActivatePromptVersionCommand</c>
    /// remarks). Absent/unparseable header means "don't check" (server always accepts the current state).
    /// </summary>
    private static uint? TryParseIfMatch(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers["If-Match"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        return uint.TryParse(header.Trim('"'), out var version) ? version : null;
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
