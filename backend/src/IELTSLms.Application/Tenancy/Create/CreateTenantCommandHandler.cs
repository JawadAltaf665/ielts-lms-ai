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

        tenants.Add(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        TenantCreated(logger, tenantId.Value, slugResult.Value.Value);
        return Result.Success(tenantId.Value);
    }

    [LoggerMessage(EventId = 3000, Level = LogLevel.Information,
        Message = "Tenant created: {TenantId} (slug: {Slug})")]
    private static partial void TenantCreated(ILogger logger, Guid tenantId, string slug);
}
