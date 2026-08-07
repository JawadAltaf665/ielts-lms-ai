using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.AI.QuestionVariants.Commands.ApproveQuestionVariant;
using IELTSLms.Application.AI.QuestionVariants.Commands.RejectQuestionVariant;
using IELTSLms.Application.AI.QuestionVariants.Queries.GetQuestionVariant;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the review endpoints for AI-generated <see cref="Domain.AI.QuestionVariantDraft"/>s (Sprint 8
/// scope item 3) — shared by both Reading and Listening, since a draft's review lifecycle doesn't
/// depend on which bounded context it came from (see <see cref="ReadingEndpoints"/>/
/// <see cref="ListeningEndpoints"/> for the per-context generate endpoints). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="GrammarEndpoints"/>.
/// </summary>
internal static class QuestionVariantEndpoints
{
    public static IEndpointRouteBuilder MapQuestionVariantEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/question-variants/{id:guid}", GetQuestionVariantAsync)
            .RequireAuthorization()
            .WithTags("QuestionVariants")
            .WithName("GetQuestionVariant")
            .WithSummary("Gets a draft variant (content.manage).");

        builder.MapPost("/question-variants/{id:guid}/approve", ApproveQuestionVariantAsync)
            .RequireAuthorization()
            .WithTags("QuestionVariants")
            .WithName("ApproveQuestionVariant")
            .WithSummary(
                "Approves a draft, promoting it into a real question via the existing content-authoring commands (content.manage).");

        builder.MapPost("/question-variants/{id:guid}/reject", RejectQuestionVariantAsync)
            .RequireAuthorization()
            .WithTags("QuestionVariants")
            .WithName("RejectQuestionVariant")
            .WithSummary("Rejects a draft; the row is retained for audit (content.manage).");

        return builder;
    }

    private static async Task<IResult> GetQuestionVariantAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetQuestionVariantQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ApproveQuestionVariantAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new ApproveQuestionVariantCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RejectQuestionVariantAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new RejectQuestionVariantCommand(tenantId, id), cancellationToken);
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
