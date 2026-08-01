using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;
using IELTSLms.Application.Speaking.Commands.OverrideSpeakingEvaluation;
using IELTSLms.Application.Speaking.Commands.SubmitSpeakingAttempt;
using IELTSLms.Application.Speaking.Queries.GetSpeakingAttempt;
using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;
using IELTSLms.Application.Speaking.Queries.GetSpeakingTask;
using IELTSLms.Application.Speaking.Queries.GetSpeakingTasks;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Speaking bounded context endpoints (Phase 6 Sprint 7, DDD §13.13 BC-13). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="WritingEndpoints"/>/<see cref="ListeningEndpoints"/>.
/// Evaluation status is polled through the existing shared <c>GET /operations/{id}</c> endpoint
/// (<see cref="AiEndpoints"/>), not a bespoke one, per the Writing precedent.
/// </summary>
internal static class SpeakingEndpoints
{
    public static IEndpointRouteBuilder MapSpeakingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/speaking-tasks", GetSpeakingTasksAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("GetSpeakingTasks")
            .WithSummary("Lists speaking tasks visible to the caller's tenant, filterable by part.");

        builder.MapGet("/speaking-tasks/{id:guid}", GetSpeakingTaskAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("GetSpeakingTask")
            .WithSummary("Gets a speaking task.");

        builder.MapPost("/speaking-tasks", CreateSpeakingTaskAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("CreateSpeakingTask")
            .WithSummary("Creates a tenant-authored speaking task (speaking.manage).");

        builder.MapPost("/speaking-attempts", CreateSpeakingAttemptAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithTags("Speaking")
            .WithName("CreateSpeakingAttempt")
            .WithSummary("Creates a speaking attempt via multipart recording upload (speaking.submit).");

        builder.MapPost("/speaking-attempts/{id:guid}/submit", SubmitSpeakingAttemptAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("SubmitSpeakingAttempt")
            .WithSummary(
                "Marks the recording complete and kicks off transcription + evaluation (speaking.submit). " +
                "Returns 202 + an operation resource pollable at GET /operations/{id}.");

        builder.MapGet("/speaking-attempts/{id:guid}", GetSpeakingAttemptAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("GetSpeakingAttempt")
            .WithSummary("Gets one of the caller's own attempts (+ evaluation, once complete), or (teacher) any attempt.");

        builder.MapGet("/speaking-evaluations/{id:guid}", GetSpeakingEvaluationAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("GetSpeakingEvaluation")
            .WithSummary("Gets evaluation detail: the four criteria, estimated band, feedback, and confidence.");

        builder.MapPatch("/speaking-evaluations/{id:guid}", OverrideSpeakingEvaluationAsync)
            .RequireAuthorization()
            .WithTags("Speaking")
            .WithName("OverrideSpeakingEvaluation")
            .WithSummary("Teacher override of the AI-produced scores (speaking.evaluate); audited, original AI scores retained.");

        return builder;
    }

    private static async Task<IResult> GetSpeakingTasksAsync(
        [AsParameters] GetSpeakingTasksQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<SpeakingPart>(queryParams.Part, "part", out var part, out var partError))
        {
            return partError!.ToProblem(httpContext);
        }

        var query = new GetSpeakingTasksQuery(
            tenantId, part, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetSpeakingTaskAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetSpeakingTaskQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateSpeakingTaskAsync(
        CreateSpeakingTaskRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<SpeakingPart>(request.Part, "part", out var part, out var partError))
        {
            return partError!.ToProblem(httpContext);
        }

        var command = new CreateSpeakingTaskCommand(tenantId, request.Title, part, request.PromptText, request.CategoryId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/speaking-tasks/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    /// <summary>
    /// Reads the multipart form directly rather than relying on model-binding attributes — same raw
    /// multipart pattern as <see cref="ListeningEndpoints.CreateListeningAudioAsync"/>, reused exactly
    /// for the learner-recorded audio upload.
    /// </summary>
    private static async Task<IResult> CreateSpeakingAttemptAsync(
        HttpContext httpContext, ISender sender, ITenantContext tenantContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        if (!httpContext.Request.HasFormContentType)
        {
            return Error.Validation("VALIDATION_SPEAKING_MULTIPART_REQUIRED", "This endpoint requires multipart/form-data.")
                .ToProblem(httpContext);
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken);

        var audioFile = form.Files["audio"];
        if (audioFile is null || audioFile.Length == 0)
        {
            return Error.Validation("VALIDATION_SPEAKING_FILE_REQUIRED", "An audio recording is required.").ToProblem(httpContext);
        }

        if (!Guid.TryParse(form["speakingTaskId"], out var speakingTaskId))
        {
            return Error.Validation("VALIDATION_SPEAKING_TASK_ID_INVALID", "speakingTaskId must be a valid id.")
                .ToProblem(httpContext);
        }

        if (!int.TryParse(form["durationSeconds"], out var durationSeconds))
        {
            return Error.Validation("VALIDATION_SPEAKING_DURATION_INVALID", "durationSeconds must be a valid integer.")
                .ToProblem(httpContext);
        }

        await using var stream = audioFile.OpenReadStream();
        var command = new CreateSpeakingAttemptCommand(
            tenantId, userId, speakingTaskId, durationSeconds, stream, audioFile.FileName, audioFile.ContentType, audioFile.Length);

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/speaking-attempts/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitSpeakingAttemptAsync(
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

        var result = await sender.Send(new SubmitSpeakingAttemptCommand(tenantId, userId, id), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem(httpContext);
        }

        return Results.Accepted($"/api/v1/operations/{result.Value.OperationId}", ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetSpeakingAttemptAsync(
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

        var result = await sender.Send(new GetSpeakingAttemptQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetSpeakingEvaluationAsync(
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

        var result = await sender.Send(new GetSpeakingEvaluationQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> OverrideSpeakingEvaluationAsync(
        Guid id, OverrideSpeakingEvaluationRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new OverrideSpeakingEvaluationCommand(
            tenantId, userId, id, request.FluencyCoherence, request.LexicalResource, request.GrammaticalRangeAccuracy,
            request.Pronunciation, request.Reason);
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
            $"VALIDATION_SPEAKING_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
