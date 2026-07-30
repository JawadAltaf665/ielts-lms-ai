using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Application.Vocabulary.DeleteVocabularyWord;
using IELTSLms.Application.Vocabulary.GetNextReviewBatch;
using IELTSLms.Application.Vocabulary.GetVocabularyProgress;
using IELTSLms.Application.Vocabulary.GetVocabularyWord;
using IELTSLms.Application.Vocabulary.GetVocabularyWords;
using IELTSLms.Application.Vocabulary.GradeVocabularyReview;
using IELTSLms.Application.Vocabulary.UpdateVocabularyWord;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Vocabulary bounded context endpoints (Phase 6 Sprint 2, DDD §5 BC-08). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="GrammarEndpoints"/>.
/// </summary>
internal static class VocabularyEndpoints
{
    public static IEndpointRouteBuilder MapVocabularyEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/vocabulary-words", GetVocabularyWordsAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("GetVocabularyWords")
            .WithSummary("Lists vocabulary words visible to the caller's tenant, filterable by topic, difficulty, and search.");

        builder.MapGet("/vocabulary-words/{id:guid}", GetVocabularyWordAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("GetVocabularyWord")
            .WithSummary("Gets a vocabulary word.");

        builder.MapPost("/vocabulary-words", CreateVocabularyWordAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("CreateVocabularyWord")
            .WithSummary("Creates a tenant-authored vocabulary word (vocabulary.manage).");

        builder.MapPatch("/vocabulary-words/{id:guid}", UpdateVocabularyWordAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("UpdateVocabularyWord")
            .WithSummary("Updates a tenant-owned vocabulary word (vocabulary.manage).");

        builder.MapDelete("/vocabulary-words/{id:guid}", DeleteVocabularyWordAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("DeleteVocabularyWord")
            .WithSummary("Soft-deletes a tenant-owned vocabulary word (vocabulary.manage).");

        builder.MapGet("/vocabulary-review-sessions/next", GetNextReviewBatchAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("GetNextVocabularyReviewBatch")
            .WithSummary("Gets the caller's next due review batch (vocabulary.review).");

        builder.MapPost("/vocabulary-review-sessions/{wordId:guid}/grade", GradeVocabularyReviewAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("GradeVocabularyReview")
            .WithSummary("Submits a recall grade (0-5) for one word, advancing its SM-2 schedule (vocabulary.review).");

        builder.MapGet("/users/{userId:guid}/vocabulary-progress", GetVocabularyProgressAsync)
            .RequireAuthorization()
            .WithTags("Vocabulary")
            .WithName("GetVocabularyProgress")
            .WithSummary("Gets the caller's own vocabulary progress summary across words.");

        return builder;
    }

    private static async Task<IResult> GetVocabularyWordsAsync(
        [AsParameters] GetVocabularyWordsQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<VocabularyDifficultyLevel>(
            queryParams.DifficultyLevel, "difficultyLevel", out var difficultyLevel, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var query = new GetVocabularyWordsQuery(
            tenantId, queryParams.Topic, difficultyLevel, queryParams.Q, queryParams.Page ?? 1,
            queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetVocabularyWordAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetVocabularyWordQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateVocabularyWordAsync(
        CreateVocabularyWordRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<VocabularyFrequencyBand>(request.FrequencyBand, "frequencyBand", out var frequencyBand, out var frequencyError))
        {
            return frequencyError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<VocabularyDifficultyLevel>(request.DifficultyLevel, "difficultyLevel", out var difficultyLevel, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new CreateVocabularyWordCommand(
            tenantId, request.Word, request.Sense, request.Meaning, request.Examples, frequencyBand, request.Topic,
            request.CategoryId, difficultyLevel);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/vocabulary-words/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> UpdateVocabularyWordAsync(
        Guid id, UpdateVocabularyWordRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<VocabularyFrequencyBand>(request.FrequencyBand, "frequencyBand", out var frequencyBand, out var frequencyError))
        {
            return frequencyError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<VocabularyDifficultyLevel>(request.DifficultyLevel, "difficultyLevel", out var difficultyLevel, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new UpdateVocabularyWordCommand(
            tenantId, id, request.Sense, request.Meaning, request.Examples, frequencyBand, request.Topic, request.CategoryId,
            difficultyLevel);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> DeleteVocabularyWordAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new DeleteVocabularyWordCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetNextReviewBatchAsync(
        [AsParameters] GetNextReviewBatchQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var result = await sender.Send(new GetNextReviewBatchQuery(tenantId, userId, queryParams.BatchSize), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GradeVocabularyReviewAsync(
        Guid wordId, GradeVocabularyReviewRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var command = new GradeVocabularyReviewCommand(tenantId, userId, wordId, request.Grade);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetVocabularyProgressAsync(
        Guid userId, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var callerUserId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetVocabularyProgressQuery(tenantId, callerUserId, userId), cancellationToken);
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
            $"VALIDATION_VOCABULARY_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
