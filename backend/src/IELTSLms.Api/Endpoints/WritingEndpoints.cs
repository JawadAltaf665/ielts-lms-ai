using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;
using IELTSLms.Application.Writing.Commands.OverrideWritingEvaluation;
using IELTSLms.Application.Writing.Commands.ResolveWritingEvaluationReview;
using IELTSLms.Application.Writing.Commands.SubmitWriting;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;
using IELTSLms.Application.Writing.Queries.GetWritingReviewQueue;
using IELTSLms.Application.Writing.Queries.GetWritingSubmission;
using IELTSLms.Application.Writing.Queries.GetWritingTask;
using IELTSLms.Application.Writing.Queries.GetWritingTasks;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Writing bounded context endpoints (Phase 6 Sprint 6, DDD §13.12 BC-12). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="ReadingEndpoints"/>/<see cref="ListeningEndpoints"/>.
/// Evaluation status is polled through the existing shared <c>GET /operations/{id}</c> endpoint
/// (<see cref="AiEndpoints"/>), not a bespoke one, per the Grammar Coach precedent.
/// </summary>
internal static class WritingEndpoints
{
    public static IEndpointRouteBuilder MapWritingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/writing-tasks", GetWritingTasksAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("GetWritingTasks")
            .WithSummary("Lists writing tasks visible to the caller's tenant, filterable by track and taskType.");

        builder.MapGet("/writing-tasks/{id:guid}", GetWritingTaskAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("GetWritingTask")
            .WithSummary("Gets a writing task.");

        builder.MapPost("/writing-tasks", CreateWritingTaskAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("CreateWritingTask")
            .WithSummary("Creates a tenant-authored writing task (writing.manage).");

        builder.MapPost("/writing-submissions", SubmitWritingAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("SubmitWriting")
            .WithSummary(
                "Submits a response for AI evaluation (writing.submit); server recomputes the word count. " +
                "Returns 202 + an operation resource pollable at GET /operations/{id}.");

        builder.MapGet("/writing-submissions/{id:guid}", GetWritingSubmissionAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("GetWritingSubmission")
            .WithSummary("Gets one of the caller's own submissions, or (teacher) any submission in the tenant.");

        builder.MapGet("/writing-evaluations/{id:guid}", GetWritingEvaluationAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("GetWritingEvaluation")
            .WithSummary("Gets evaluation detail: the four criterion scores, estimated band, feedback, and confidence.");

        builder.MapPatch("/writing-evaluations/{id:guid}", OverrideWritingEvaluationAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("OverrideWritingEvaluation")
            .WithSummary("Teacher override of the AI-produced scores (writing.evaluate); audited, original AI scores retained.");

        builder.MapGet("/writing-evaluations/review-queue", GetWritingReviewQueueAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("GetWritingReviewQueue")
            .WithSummary(
                "Lists flagged/low-confidence/high-similarity evaluations awaiting human review (writing.evaluate), cursor-paginated.");

        builder.MapPost("/writing-evaluations/{id:guid}/resolve-review", ResolveWritingEvaluationReviewAsync)
            .RequireAuthorization()
            .WithTags("Writing")
            .WithName("ResolveWritingEvaluationReview")
            .WithSummary(
                "Teacher marks a flagged evaluation reviewed, with or without a score override (writing.evaluate); " +
                "audited (actor, timestamp, decision), distinct from PATCH.");

        return builder;
    }

    private static async Task<IResult> GetWritingTasksAsync(
        [AsParameters] GetWritingTasksQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<ReadingTrack>(queryParams.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<WritingTaskType>(queryParams.TaskType, "taskType", out var taskType, out var taskTypeError))
        {
            return taskTypeError!.ToProblem(httpContext);
        }

        var query = new GetWritingTasksQuery(
            tenantId, track, taskType, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetWritingTaskAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetWritingTaskQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateWritingTaskAsync(
        CreateWritingTaskRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<WritingTaskType>(request.TaskType, "taskType", out var taskType, out var taskTypeError))
        {
            return taskTypeError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingTrack>(request.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        var command = new CreateWritingTaskCommand(
            tenantId, request.Title, taskType, track, request.PromptText, request.RubricReference, request.MinWordCount,
            request.MaxWordCount, request.CategoryId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/writing-tasks/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitWritingAsync(
        SubmitWritingRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new SubmitWritingCommand(tenantId, userId, request.WritingTaskId, request.ResponseText);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem(httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.OperationId}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetWritingSubmissionAsync(
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

        var result = await sender.Send(new GetWritingSubmissionQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetWritingEvaluationAsync(
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

        var result = await sender.Send(new GetWritingEvaluationQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> OverrideWritingEvaluationAsync(
        Guid id, OverrideWritingEvaluationRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new OverrideWritingEvaluationCommand(
            tenantId, userId, id, request.TaskResponse, request.CoherenceCohesion, request.LexicalResource,
            request.GrammaticalRangeAccuracy, request.Reason);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetWritingReviewQueueAsync(
        [AsParameters] GetWritingReviewQueueQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var limit = PaginationDefaults.NormalizePageSize(queryParams.Limit ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(new GetWritingReviewQueueQuery(tenantId, queryParams.Cursor, limit), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ResolveWritingEvaluationReviewAsync(
        Guid id, ResolveWritingEvaluationReviewRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new ResolveWritingEvaluationReviewCommand(tenantId, userId, id, request.Decision);
        var result = await sender.Send(command, cancellationToken);
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

    private static bool TryParseEnum<TEnum>(string? value, string fieldName, out TEnum result, out Error? error)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse(value, ignoreCase: true, out result))
        {
            error = null;
            return true;
        }

        error = Error.Validation(
            $"VALIDATION_WRITING_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
        return false;
    }

    private static bool TryParseOptionalEnum<TEnum>(string? value, string fieldName, out TEnum? result, out Error? error)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            error = null;
            return true;
        }

        if (TryParseEnum<TEnum>(value, fieldName, out var parsed, out error))
        {
            result = parsed;
            return true;
        }

        result = null;
        return false;
    }

    private static readonly Error TenantNotResolved = Error.Failure(
        "AUTHZ_TENANT_NOT_RESOLVED", "No tenant could be resolved for this request.");

    private static readonly Error UserNotResolved = Error.Failure(
        "AUTHZ_USER_NOT_RESOLVED", "No user could be resolved for this request.");
}
