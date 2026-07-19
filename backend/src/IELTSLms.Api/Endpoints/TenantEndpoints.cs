using IELTSLms.Api.Contracts;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Tenancy;
using IELTSLms.Application.Tenancy.Activate;
using IELTSLms.Application.Tenancy.Create;
using IELTSLms.Application.Tenancy.Deactivate;
using IELTSLms.Application.Tenancy.Delete;
using IELTSLms.Application.Tenancy.GetById;
using IELTSLms.Application.Tenancy.GetBySlug;
using IELTSLms.Application.Tenancy.List;
using IELTSLms.Application.Tenancy.Restore;
using IELTSLms.Application.Tenancy.Update;
using MediatR;

namespace IELTSLms.Api.Endpoints;

internal static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/tenants")
            .WithTags("Tenants");

        group.MapPost("/", CreateTenantAsync)
            .AllowAnonymous()
            .WithName("CreateTenant")
            .WithSummary("Provisions a new tenant.");

        group.MapGet("/", ListTenantsAsync)
            .AllowAnonymous()
            .WithName("ListTenants")
            .WithSummary("Lists tenants with offset pagination.");

        group.MapGet("/{id:guid}", GetTenantByIdAsync)
            .AllowAnonymous()
            .WithName("GetTenantById")
            .WithSummary("Gets a tenant by its identifier.");

        group.MapGet("/by-slug/{slug}", GetTenantBySlugAsync)
            .AllowAnonymous()
            .WithName("GetTenantBySlug")
            .WithSummary("Gets a tenant by its URL-safe slug.");

        group.MapPut("/{id:guid}", UpdateTenantAsync)
            .AllowAnonymous()
            .WithName("UpdateTenant")
            .WithSummary("Updates a tenant's profile information.");

        group.MapDelete("/{id:guid}", DeleteTenantAsync)
            .AllowAnonymous()
            .WithName("DeleteTenant")
            .WithSummary("Soft-deletes a tenant.");

        group.MapPost("/{id:guid}/activate", ActivateTenantAsync)
            .AllowAnonymous()
            .WithName("ActivateTenant")
            .WithSummary("Reactivates a suspended tenant.");

        group.MapPost("/{id:guid}/deactivate", DeactivateTenantAsync)
            .AllowAnonymous()
            .WithName("DeactivateTenant")
            .WithSummary("Suspends a tenant.");

        group.MapPost("/{id:guid}/restore", RestoreTenantAsync)
            .AllowAnonymous()
            .WithName("RestoreTenant")
            .WithSummary("Restores a soft-deleted tenant.");

        return builder;
    }

    private static async Task<IResult> CreateTenantAsync(
        CreateTenantRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateTenantCommand(
            request.Slug,
            request.Code,
            request.Name,
            request.PrimaryDomain,
            request.ContactEmail,
            request.DefaultLocale,
            request.DefaultTimeZone);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Created(
            $"/api/v1/tenants/{result.Value}",
            ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> ListTenantsAsync(
        [AsParameters] ListTenantsQueryParams queryParams,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new ListTenantsQuery(queryParams.Page ?? 1, queryParams.PageSize ?? 20);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Ok(ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetTenantByIdAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Ok(ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> GetTenantBySlugAsync(
        string slug,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantBySlugQuery(slug), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Ok(ApiResponse.From(result.Value, httpContext));
    }

    private static async Task<IResult> UpdateTenantAsync(
        Guid id,
        UpdateTenantRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTenantCommand(
            id,
            request.Name,
            request.PrimaryDomain,
            request.ContactEmail,
            request.DefaultLocale,
            request.DefaultTimeZone);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.Ok(ApiResponse.Empty(httpContext));
    }

    private static async Task<IResult> DeleteTenantAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTenantCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ActivateTenantAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ActivateTenantCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateTenantAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateTenantCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> RestoreTenantAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RestoreTenantCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return Results.NoContent();
    }
}

public sealed record ListTenantsQueryParams(int? Page, int? PageSize);
