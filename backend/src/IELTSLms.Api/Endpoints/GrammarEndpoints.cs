using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Grammar.CreateGrammarExercise;
using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Application.Grammar.DeleteGrammarTopic;
using IELTSLms.Application.Grammar.GetGrammarAttempt;
using IELTSLms.Application.Grammar.GetGrammarExercise;
using IELTSLms.Application.Grammar.GetGrammarProgress;
using IELTSLms.Application.Grammar.GetGrammarTopic;
using IELTSLms.Application.Grammar.GetGrammarTopics;
using IELTSLms.Application.Grammar.SubmitGrammarAttempt;
using IELTSLms.Application.Grammar.UpdateGrammarTopic;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Grammar bounded context endpoints (Phase 6 Sprint 1, DDD §13.7 BC-07). Every route is
/// tenant-scoped and requires authentication, mirroring <see cref="BillingEndpoints"/>.
/// </summary>
internal static class GrammarEndpoints
{
    public static IEndpointRouteBuilder MapGrammarEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/grammar-topics", GetGrammarTopicsAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("GetGrammarTopics")
            .WithSummary("Lists grammar topics visible to the caller's tenant, filterable by track and difficulty.");

        builder.MapGet("/grammar-topics/{id:guid}", GetGrammarTopicAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("GetGrammarTopic")
            .WithSummary("Gets a grammar topic and its exercises.");

        builder.MapPost("/grammar-topics", CreateGrammarTopicAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("CreateGrammarTopic")
            .WithSummary("Creates a tenant-authored grammar topic (grammar.manage).");

        builder.MapPatch("/grammar-topics/{id:guid}", UpdateGrammarTopicAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("UpdateGrammarTopic")
            .WithSummary("Updates a tenant-owned grammar topic (grammar.manage).");

        builder.MapDelete("/grammar-topics/{id:guid}", DeleteGrammarTopicAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("DeleteGrammarTopic")
            .WithSummary("Soft-deletes a tenant-owned grammar topic (grammar.manage).");

        builder.MapPost("/grammar-topics/{id:guid}/exercises", CreateGrammarExerciseAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("CreateGrammarExercise")
            .WithSummary("Creates an exercise (with its questions) under a topic (grammar.manage).");

        builder.MapGet("/grammar-exercises/{id:guid}", GetGrammarExerciseAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("GetGrammarExercise")
            .WithSummary("Gets an exercise and its questions. Answer keys are never included.");

        builder.MapPost("/grammar-exercises/{id:guid}/attempts", SubmitGrammarAttemptAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("SubmitGrammarAttempt")
            .WithSummary("Submits answers for synchronous, rule-based grading (grammar.attempt).");

        builder.MapGet("/grammar-attempts/{id:guid}", GetGrammarAttemptAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("GetGrammarAttempt")
            .WithSummary("Gets one of the caller's own past attempts and its result.");

        builder.MapGet("/users/{userId:guid}/grammar-progress", GetGrammarProgressAsync)
            .RequireAuthorization()
            .WithTags("Grammar")
            .WithName("GetGrammarProgress")
            .WithSummary("Gets the caller's own grammar progress summary across topics.");

        return builder;
    }

    private static async Task<IResult> GetGrammarTopicsAsync(
        [AsParameters] GetGrammarTopicsQueryParams queryParams, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<GrammarTrack>(queryParams.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseOptionalEnum<GrammarDifficulty>(queryParams.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var query = new GetGrammarTopicsQuery(tenantId, track, difficulty, queryParams.Page ?? 1, queryParams.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetGrammarTopicAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetGrammarTopicQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateGrammarTopicAsync(
        CreateGrammarTopicRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<GrammarTrack>(request.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<GrammarDifficulty>(request.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new CreateGrammarTopicCommand(tenantId, request.Title, request.Description, track, difficulty, request.CategoryId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/grammar-topics/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> UpdateGrammarTopicAsync(
        Guid id, UpdateGrammarTopicRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<GrammarTrack>(request.Track, "track", out var track, out var trackError))
        {
            return trackError!.ToProblem(httpContext);
        }

        if (!TryParseEnum<GrammarDifficulty>(request.Difficulty, "difficulty", out var difficulty, out var difficultyError))
        {
            return difficultyError!.ToProblem(httpContext);
        }

        var command = new UpdateGrammarTopicCommand(tenantId, id, request.Title, request.Description, track, difficulty, request.CategoryId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> DeleteGrammarTopicAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new DeleteGrammarTopicCommand(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToProblem(httpContext);
    }

    private static async Task<IResult> CreateGrammarExerciseAsync(
        Guid id, CreateGrammarExerciseRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryParseEnum<GrammarExerciseMode>(request.Mode, "mode", out var mode, out var modeError))
        {
            return modeError!.ToProblem(httpContext);
        }

        var questions = new List<CreateGrammarQuestionRequest>(request.Questions.Count);
        foreach (var q in request.Questions)
        {
            if (!TryParseEnum<GrammarQuestionType>(q.Type, "type", out var type, out var typeError))
            {
                return typeError!.ToProblem(httpContext);
            }

            questions.Add(new CreateGrammarQuestionRequest(type, q.Prompt, q.AnswerKey, q.Sequence));
        }

        var command = new CreateGrammarExerciseCommand(tenantId, id, request.Title, mode, request.Order, questions);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/grammar-exercises/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetGrammarExerciseAsync(
        Guid id, ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetGrammarExerciseQuery(tenantId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitGrammarAttemptAsync(
        Guid id, SubmitGrammarAttemptRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        var answers = request.Answers.Select(a => new GrammarAnswerSubmission(a.QuestionId, a.Answer)).ToArray();
        var command = new SubmitGrammarAttemptCommand(tenantId, userId, id, answers);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetGrammarAttemptAsync(
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

        var result = await sender.Send(new GetGrammarAttemptQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetGrammarProgressAsync(
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

        var result = await sender.Send(new GetGrammarProgressQuery(tenantId, callerUserId, userId), cancellationToken);
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
            $"VALIDATION_GRAMMAR_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
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
