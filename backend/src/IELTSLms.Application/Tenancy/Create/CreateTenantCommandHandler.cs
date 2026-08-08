using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Create;

public sealed partial class CreateTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    IGuidProvider guids,
    ILogger<CreateTenantCommandHandler> logger)
    : ICommandHandler<CreateTenantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var slugResult = TenantSlug.Create(request.Slug);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Guid>(TenantErrors.InvalidSlug);
        }

        var codeResult = TenantCode.Create(request.Code);
        if (codeResult.IsFailure)
        {
            return Result.Failure<Guid>(TenantErrors.InvalidCode);
        }

        if (await tenants.SlugExistsAsync(slugResult.Value.Value, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<Guid>(TenantErrors.SlugTaken);
        }

        if (await tenants.CodeExistsAsync(codeResult.Value.Value, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<Guid>(TenantErrors.CodeTaken);
        }

        var tenantId = TenantId.From(guids.NewGuid());

        var tenant = Tenant.Create(
            tenantId,
            slugResult.Value,
            codeResult.Value,
            request.Name,
            clock.UtcNow,
            request.PrimaryDomain,
            request.ContactEmail,
            request.DefaultLocale ?? Tenant.FallbackLocale,
            request.DefaultTimeZone ?? Tenant.FallbackTimeZone);

        // No per-tenant RBAC provisioning needed here: the system roles (Role.TenantId == null) and
        // their permission grants are global rows, seeded once (RbacCatalogSeeder) — every tenant's
        // users resolve permissions from the same grants (Auth §9.2; see IRolePermissionRepository's
        // remarks on why grants aren't duplicated per tenant). A tenant that wants its own custom
        // roles (Role.TenantId == this tenant) creates and grants them explicitly (role.assign, not
        // part of this command).
        tenants.Add(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        TenantCreated(logger, tenantId.Value, slugResult.Value.Value);
        return Result.Success(tenantId.Value);
    }

    [LoggerMessage(EventId = 3000, Level = LogLevel.Information,
        Message = "Tenant created: {TenantId} (slug: {Slug})")]
    private static partial void TenantCreated(ILogger logger, Guid tenantId, string slug);
}
