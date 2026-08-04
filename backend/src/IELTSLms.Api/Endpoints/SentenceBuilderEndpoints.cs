using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;
using IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;
using IELTSLms.Application.SentenceBuilder.Queries.GetSentenceAttempt;
using IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercise;
using IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercises;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Sentence Builder bounded context endpoints (Phase 7 Sprint 7 — the base module, standing up
/// the gap Phase 6 left, see the sprint's own gap note). Every route is tenant-scoped and requires
/// authentication, mirroring <see cref="GrammarEndpoints"/>.
/// </summary>
internal static class SentenceBuilderEndpoints
{
    public static IEndpointRouteBuilder MapSentenceBuilderEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/sentence-exercises", GetSentenceExercisesAsync)
            .RequireAuthorization()
            .WithTags("SentenceBuilder")
            .WithName("GetSentenceExercises")
            .WithSummary("Lists sentence-construction exercises visible to the caller's tenant, filterable by difficulty.");

        builder.MapGet("/sentence-exercises/{id:guid}", GetSentenceExerciseAsync)
            .RequireAuthorization()
            .WithTags("SentenceBuilder")
            .WithName("GetSentenceExercise")
            .WithSummary("Gets a sentence-construction exercise.");

        builder.MapPost("/sentence-exercises", CreateSentenceExerciseAsync)
            .RequireAuthorization()
            .WithTags("SentenceBuilder")
            .WithName("CreateSentenceExercise")
            .WithSummary("Creates a tenant-authored sentence-construction exercise (sentence-builder.manage).");

        builder.MapPost("/sentence-exercises/{id:guid}/attempts", SubmitSentenceAttemptAsync)
            .RequireAuthorization()
            .WithTags("SentenceBuilder")
            .WithName("SubmitSentenceAttempt")
            .WithSummary(
                "Submits a constructed sentence for synchronous deterministic pattern-match scoring " +
                "(sentence-builder.attempt); AI scaffolding feedback attaches asynchronously.");

        builder.MapGet("/sentence-attempts/{id:guid}", GetSentenceAttemptAsync)
            .RequireAuthorization()
            .WithTags("SentenceBuilder")
            .WithName("GetSentenceAttempt")
            .WithSummary("Gets one of the caller's own past attempts and its result, including AI feedback once ready.");

        return builder;
    }

    private static async Task<IResult> GetSentenceExercisesAsync(
        [AsParameters] GetSentenceExercisesQueryParams queryParams, ISender sender, ITenantContext tenantContext,
        HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<SentenceDifficulty>(queryParams.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var query = new GetSentenceExercisesQuery(
            tenantId, difficulty, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetSentenceExerciseAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetSentenceExerciseQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateSentenceExerciseAsync(
        CreateSentenceExerciseRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<SentenceDifficulty>(request.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new CreateSentenceExerciseCommand(tenantId, request.Prompt, request.TargetPattern, difficulty);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/sentence-exercises/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitSentenceAttemptAsync(
        Guid id, SubmitSentenceAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new SubmitSentenceAttemptCommand(tenantId, userId, id, request.SubmittedText);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetSentenceAttemptAsync(
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

        var result = await sender.Send(new GetSentenceAttemptQuery(tenantId, userId, id), cancellationToken);
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
            $"VALIDATION_SENTENCE_BUILDER_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
