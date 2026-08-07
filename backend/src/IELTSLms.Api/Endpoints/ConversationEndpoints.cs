using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.Conversations.Commands.ArchiveConversation;
using IELTSLms.Application.AI.Conversations.Queries.GetConversation;
using IELTSLms.Application.AI.Conversations.Queries.ListConversations;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Memory &amp; Conversation History read/manage endpoints (Phase 7 Sprint 3, AI Architecture §6):
/// list/get a learner's own conversations and archive one. Self-scoped only — the message-append/turn-
/// taking endpoints belong to Sprint 4's Tutor/Teacher capabilities, not here.
/// </summary>
internal static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/conversations", ListConversationsAsync)
            .RequireAuthorization()
            .WithTags("Conversations")
            .WithName("ListConversations")
            .WithSummary("Lists the caller's own conversations, cursor-paginated, filterable by feature/status (ai.conversation.view).");

        builder.MapGet("/conversations/{id:guid}", GetConversationAsync)
            .RequireAuthorization()
            .WithTags("Conversations")
            .WithName("GetConversation")
            .WithSummary("Gets one of the caller's own conversations plus a cursor page of its message history (ai.conversation.view).");

        builder.MapDelete("/conversations/{id:guid}", ArchiveConversationAsync)
            .RequireAuthorization()
            .WithTags("Conversations")
            .WithName("ArchiveConversation")
            .WithSummary("Archives (soft-deletes) one of the caller's own conversations (ai.conversation.view).");

        return builder;
    }

    private static async Task<IResult> ListConversationsAsync(
        [AsParameters] ListConversationsQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var limit = PaginationDefaults.NormalizePageSize(queryParams.Limit ?? PaginationDefaults.DefaultPageSize);
        var query = new ListConversationsQuery(tenantId, userId, queryParams.Feature, queryParams.Status, queryParams.Cursor, limit);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetConversationAsync(
        Guid id, [AsParameters] GetConversationQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var limit = PaginationDefaults.NormalizePageSize(queryParams.Limit ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(new GetConversationQuery(tenantId, userId, id, queryParams.Cursor, limit), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ArchiveConversationAsync(
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

        var result = await sender.Send(new ArchiveConversationCommand(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
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
