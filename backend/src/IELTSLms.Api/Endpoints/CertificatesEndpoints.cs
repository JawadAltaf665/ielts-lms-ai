using System.Security.Claims;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Certificates.GetCertificate;
using IELTSLms.Application.Certificates.VerifyCertificate;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the Certificates endpoints (Phase 6 Sprint 9, DDD §13.16 BC-16). Verification is the phase's
/// one deliberate public/unauthenticated exception (API Design Standards §16, ERD ENT-038) — no
/// <c>.RequireAuthorization()</c> on that route, unlike every other endpoint in this codebase.
/// </summary>
internal static class CertificatesEndpoints
{
    public static IEndpointRouteBuilder MapCertificatesEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/certificates/{id:guid}", GetCertificateAsync)
            .RequireAuthorization()
            .WithTags("Certificates")
            .WithName("GetCertificate")
            .WithSummary("Gets certificate detail for its owner or a teacher (certificate.view).");

        builder.MapGet("/certificates/verify/{code}", VerifyCertificateAsync)
            .WithTags("Certificates")
            .WithName("VerifyCertificate")
            .WithSummary("Public, unauthenticated certificate verification by code. Returns minimal, non-sensitive fields only.");

        return builder;
    }

    private static async Task<IResult> GetCertificateAsync(
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

        var result = await sender.Send(new GetCertificateQuery(tenantId, userId, id), cancellationToken);
        return result.IsSuccess ? Results.Ok(ApiResponse.From(result.Value, httpContext)) : result.ToProblem(httpContext);
    }

    private static async Task<IResult> VerifyCertificateAsync(
        string code, ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyCertificateQuery(code), cancellationToken);
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

    private static readonly Error TenantNotResolved = Error.Failure(
        "AUTHZ_TENANT_NOT_RESOLVED", "No tenant could be resolved for this request.");

    private static readonly Error UserNotResolved = Error.Failure(
        "AUTHZ_USER_NOT_RESOLVED", "No user could be resolved for this request.");
}
