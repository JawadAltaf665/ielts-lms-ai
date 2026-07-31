using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Reading.AddReadingQuestion;
using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Application.Reading.GetReadingAttempt;
using IELTSLms.Application.Reading.GetReadingPassage;
using IELTSLms.Application.Reading.GetReadingPassages;
using IELTSLms.Application.Reading.StartReadingAttempt;
using IELTSLms.Application.Reading.SubmitReadingAttempt;
using IELTSLms.Application.Reading.UpdateReadingPassage;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Reading bounded context endpoints (Phase 6 Sprint 4, DDD §13.10 BC-10). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="GrammarEndpoints"/>.
/// </summary>
internal static class ReadingEndpoints
{
    public static IEndpointRouteBuilder MapReadingEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/reading-passages", GetReadingPassagesAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("GetReadingPassages")
            .WithSummary("Lists reading passages visible to the caller's tenant, filterable by track and difficulty.");

        builder.MapGet("/reading-passages/{id:guid}", GetReadingPassageAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("GetReadingPassage")
            .WithSummary("Gets a reading passage and its questions. Answer keys are never included.");

        builder.MapPost("/reading-passages", CreateReadingPassageAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("CreateReadingPassage")
            .WithSummary("Creates a tenant-authored reading passage (reading.manage).");

        builder.MapPatch("/reading-passages/{id:guid}", UpdateReadingPassageAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("UpdateReadingPassage")
            .WithSummary("Updates a tenant-owned reading passage (reading.manage).");

        builder.MapPost("/reading-passages/{id:guid}/questions", AddReadingQuestionAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("AddReadingQuestion")
            .WithSummary("Adds a question to a tenant-owned passage (reading.manage).");

        builder.MapPost("/reading-attempts", StartReadingAttemptAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("StartReadingAttempt")
            .WithSummary("Starts a server-timed attempt at a passage; the server owns the deadline (reading.attempt).");

        builder.MapPost("/reading-attempts/{id:guid}/submit", SubmitReadingAttemptAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("SubmitReadingAttempt")
            .WithSummary("Submits answers for synchronous grading; rejected once the server deadline has passed (reading.attempt).");

        builder.MapGet("/reading-attempts/{id:guid}", GetReadingAttemptAsync)
            .RequireAuthorization()
            .WithTags("Reading")
            .WithName("GetReadingAttempt")
            .WithSummary("Gets one of the caller's own past attempts and its result.");

        return builder;
    }

    private static async Task<IResult> GetReadingPassagesAsync(
        [AsParameters] GetReadingPassagesQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        if (!TryParseOptionalEnum<ReadingDifficulty>(queryParams.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var query = new GetReadingPassagesQuery(
            tenantId, track, difficulty, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetReadingPassageAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetReadingPassageQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateReadingPassageAsync(
        CreateReadingPassageRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingTrack>(request.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingDifficulty>(request.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new CreateReadingPassageCommand(
            tenantId, request.Title, request.PassageText, track, difficulty, request.CategoryId, request.DurationSeconds);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/reading-passages/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> UpdateReadingPassageAsync(
        Guid id, UpdateReadingPassageRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingTrack>(request.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<ReadingDifficulty>(request.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new UpdateReadingPassageCommand(
            tenantId, id, request.Title, request.PassageText, track, difficulty, request.CategoryId, request.DurationSeconds);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> AddReadingQuestionAsync(
        Guid id, AddReadingQuestionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new AddReadingQuestionCommand(tenantId, id, type, request.Prompt, request.AnswerKey, request.Sequence);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/reading-passages/{id}/questions/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> StartReadingAttemptAsync(
        StartReadingAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new StartReadingAttemptCommand(tenantId, userId, request.PassageId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/reading-attempts/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitReadingAttemptAsync(
        Guid id, SubmitReadingAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var answers = request.Answers.Select(a => new ReadingAnswerSubmission(a.QuestionId, a.Answer)).ToArray();
        var command = new SubmitReadingAttemptCommand(tenantId, userId, id, answers);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetReadingAttemptAsync(
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

        var result = await sender.Send(new GetReadingAttemptQuery(tenantId, userId, id), cancellationToken);
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
            $"VALIDATION_READING_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
