using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Assessment.Commands.AbandonMockExam;
using IELTSLms.Application.Assessment.Commands.StartMockExam;
using IELTSLms.Application.Assessment.Commands.StartMockExamSection;
using IELTSLms.Application.Assessment.Commands.SubmitMockExam;
using IELTSLms.Application.Assessment.Queries.GetMockExam;
using IELTSLms.Application.Assessment.Queries.GetMockExamResult;
using IELTSLms.Application.Assessment.Queries.GetOrchestratedReview;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Assessment bounded context's mock-exam endpoints (Phase 6 Sprint 8, DDD §13.5 BC-13).
/// Every route is tenant-scoped and requires authentication, mirroring <see cref="ReadingEndpoints"/>.
/// </summary>
internal static class MockExamEndpoints
{
    public static IEndpointRouteBuilder MapMockExamEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/mock-exams", StartMockExamAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("StartMockExam")
            .WithSummary("Starts a new mock exam session; 409 if an active session already exists for the user (mock-exam.attempt).");

        builder.MapGet("/mock-exams/{id:guid}", GetMockExamAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("GetMockExam")
            .WithSummary("Gets the caller's own mock exam session status and section references.");

        builder.MapPost("/mock-exams/{id:guid}/sections/{section}/start", StartMockExamSectionAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("StartMockExamSection")
            .WithSummary("Starts one section (reading|listening|writing|speaking) within the session (mock-exam.attempt).");

        builder.MapPost("/mock-exams/{id:guid}/submit", SubmitMockExamAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("SubmitMockExam")
            .WithSummary("Signals exam-level completion; finalizes the result if not already finalized (mock-exam.attempt).");

        builder.MapGet("/mock-exams/{id:guid}/result", GetMockExamResultAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("GetMockExamResult")
            .WithSummary("Gets the aggregated result; always 200, with a pending/finalized status field.");

        builder.MapPost("/mock-exams/{id:guid}/abandon", AbandonMockExamAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("AbandonMockExam")
            .WithSummary("Abandons an in-progress session, releasing the single-active-session lock (mock-exam.attempt).");

        builder.MapGet("/mock-exams/{id:guid}/orchestrated-review", GetOrchestratedReviewAsync)
            .RequireAuthorization()
            .WithTags("MockExams")
            .WithName("GetOrchestratedReview")
            .WithSummary("Retrieves the status of the Monthly Mock Exam Workflow review (Sprint 10).");

        return builder;
    }

    private static async Task<IResult> StartMockExamAsync(
        ISender sender, ITenantContext tenantContext, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(tenantContext, out var tenantId))
        {
            return TenantNotResolved.ToProblem(httpContext);
        }

        if (!TryGetUserId(httpContext, out var userId))
        {
            return UserNotResolved.ToProblem(httpContext);
        }

        var result = await sender.Send(new StartMockExamCommand(tenantId, userId), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/mock-exams/{result.Value.Id}", ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetMockExamAsync(
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

        var result = await sender.Send(new GetMockExamQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> StartMockExamSectionAsync(
        Guid id, string section, StartMockExamSectionRequest request, ISender sender, ITenantContext tenantContext, HttpContext httpContext,
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

        if (!TryParseEnum<MockExamSectionType>(section, "section", out var sectionType, out var sectionError))
        {
            return sectionError!.ToProblem(httpContext);
        }

        var command = new StartMockExamSectionCommand(tenantId, userId, id, sectionType, request.ContentId);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> SubmitMockExamAsync(
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

        var result = await sender.Send(new SubmitMockExamCommand(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetMockExamResultAsync(
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

        var result = await sender.Send(new GetMockExamResultQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> AbandonMockExamAsync(
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

        var result = await sender.Send(new AbandonMockExamCommand(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetOrchestratedReviewAsync(
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

        var result = await sender.Send(new GetOrchestratedReviewQuery(tenantId, userId, id), cancellationToken);
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
            $"VALIDATION_MOCK_EXAM_{fieldName.ToUpperInvariant()}_INVALID", $"'{value}' is not a valid {fieldName}.");
        return false;
    }

    private static readonly Error TenantNotResolved = Error.Failure(
        "AUTHZ_TENANT_NOT_RESOLVED", "No tenant could be resolved for this request.");

    private static readonly Error UserNotResolved = Error.Failure(
        "AUTHZ_USER_NOT_RESOLVED", "No user could be resolved for this request.");
}
