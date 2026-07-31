using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Listening.AddListeningQuestion;
using IELTSLms.Application.Listening.CreateListeningAudio;
using IELTSLms.Application.Listening.GetListeningAttempt;
using IELTSLms.Application.Listening.GetListeningAudio;
using IELTSLms.Application.Listening.GetListeningAudios;
using IELTSLms.Application.Listening.GetListeningAudioStream;
using IELTSLms.Application.Listening.RegisterListeningReplay;
using IELTSLms.Application.Listening.StartListeningAttempt;
using IELTSLms.Application.Listening.SubmitListeningAttempt;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Listening bounded context endpoints (Phase 6 Sprint 5, DDD §13.11 BC-11). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="ReadingEndpoints"/>.
/// </summary>
internal static class ListeningEndpoints
{
    public static IEndpointRouteBuilder MapListeningEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/listening-audios", GetListeningAudiosAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("GetListeningAudios")
            .WithSummary("Lists listening audios visible to the caller's tenant, filterable by track.");

        builder.MapGet("/listening-audios/{id:guid}", GetListeningAudioAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("GetListeningAudio")
            .WithSummary("Gets a listening audio and its questions. Answer keys are never included; transcript only when mode=learning.");

        builder.MapGet("/listening-audios/{id:guid}/stream", GetListeningAudioStreamAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("GetListeningAudioStream")
            .WithSummary("Issues a signed, expiring playback URL for the initial listen (listening.attempt).");

        builder.MapPost("/listening-audios", CreateListeningAudioAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithTags("Listening")
            .WithName("CreateListeningAudio")
            .WithSummary("Creates a tenant-authored listening audio via multipart upload (listening.manage).");

        builder.MapPost("/listening-audios/{id:guid}/questions", AddListeningQuestionAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("AddListeningQuestion")
            .WithSummary("Adds a question to a tenant-owned audio (listening.manage).");

        builder.MapPost("/listening-attempts", StartListeningAttemptAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("StartListeningAttempt")
            .WithSummary("Starts a server-timed attempt; the server owns the deadline and the mode's replay cap (listening.attempt).");

        builder.MapPost("/listening-attempts/{id:guid}/replays", RegisterListeningReplayAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("RegisterListeningReplay")
            .WithSummary("Registers a replay against the server-tracked cap and issues a fresh signed URL (listening.attempt).");

        builder.MapPost("/listening-attempts/{id:guid}/submit", SubmitListeningAttemptAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("SubmitListeningAttempt")
            .WithSummary("Submits answers for synchronous grading; rejected once the server deadline has passed (listening.attempt).");

        builder.MapGet("/listening-attempts/{id:guid}", GetListeningAttemptAsync)
            .RequireAuthorization()
            .WithTags("Listening")
            .WithName("GetListeningAttempt")
            .WithSummary("Gets one of the caller's own past attempts and its result.");

        return builder;
    }

    private static async Task<IResult> GetListeningAudiosAsync(
        [AsParameters] GetListeningAudiosQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var query = new GetListeningAudiosQuery(tenantId, track, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetListeningAudioAsync(
        Guid id, string? mode, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        // Safe default (Exam) for a missing/unrecognized mode value — only the exact "learning" enum
        // member unlocks the transcript, so no crafted query value can slip past this (Sprint 5 test note).
        var parsedMode = Enum.TryParse<ListeningMode>(mode, ignoreCase: true, out var parsed) ? parsed : ListeningMode.Exam;

        var result = await sender.Send(new GetListeningAudioQuery(tenantId, id, parsedMode), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetListeningAudioStreamAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetListeningAudioStreamQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    /// <summary>
    /// Reads the multipart form directly rather than relying on model-binding attributes — the first
    /// upload endpoint in this codebase, same "raw request access" precedent as
    /// <see cref="WebhookEndpoints"/>'s raw-body read, just for a different reason (file streaming
    /// instead of signature verification over exact bytes).
    /// </summary>
    private static async Task<IResult> CreateListeningAudioAsync(
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
            return Error.Validation("VALIDATION_LISTENING_MULTIPART_REQUIRED", "This endpoint requires multipart/form-data.")
                .ToProblem(httpContext);
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken);

        var audioFile = form.Files["audio"];
        if (audioFile is null || audioFile.Length == 0)
        {
            return Error.Validation("VALIDATION_LISTENING_FILE_REQUIRED", "An audio file is required.").ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingTrack>(form["track"], "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!int.TryParse(form["durationSeconds"], out var durationSeconds))
        {
            return Error.Validation("VALIDATION_LISTENING_DURATION_INVALID", "durationSeconds must be a valid integer.")
                .ToProblem(httpContext);
        }

        var categoryId = Guid.TryParse(form["categoryId"], out var parsedCategoryId) ? parsedCategoryId : (Guid?)null;
        var transcriptText = form["transcriptText"].ToString();
        transcriptText = string.IsNullOrWhiteSpace(transcriptText) ? null : transcriptText;

        await using var stream = audioFile.OpenReadStream();
        var command = new CreateListeningAudioCommand(
            tenantId, userId, form["title"].ToString(), track, durationSeconds, categoryId, transcriptText, stream, audioFile.FileName,
            audioFile.ContentType, audioFile.Length);

        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/listening-audios/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> AddListeningQuestionAsync(
        Guid id, AddListeningQuestionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingQuestionType>(request.Type, "type", out var type, out var typeError))
        {
            return typeError!.ToProblem(httpContext);
        }

        var command = new AddListeningQuestionCommand(tenantId, id, type, request.Prompt, request.AnswerKey, request.Sequence);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/listening-audios/{id}/questions/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> StartListeningAttemptAsync(
        StartListeningAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        if (!TryParseEnum<ListeningMode>(request.Mode, "mode", out var mode, out var modeError))
        {
            return modeError!.ToProblem(httpContext);
        }

        var command = new StartListeningAttemptCommand(tenantId, userId, request.AudioId, mode);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/listening-attempts/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RegisterListeningReplayAsync(
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

        var result = await sender.Send(new RegisterListeningReplayCommand(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitListeningAttemptAsync(
        Guid id, SubmitListeningAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var answers = request.Answers.Select(a => new ListeningAnswerSubmission(a.QuestionId, a.Answer)).ToArray();
        var command = new SubmitListeningAttemptCommand(tenantId, userId, id, answers);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetListeningAttemptAsync(
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

        var result = await sender.Send(new GetListeningAttemptQuery(tenantId, userId, id), cancellationToken);
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
            $"VALIDATION_LISTENING_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
