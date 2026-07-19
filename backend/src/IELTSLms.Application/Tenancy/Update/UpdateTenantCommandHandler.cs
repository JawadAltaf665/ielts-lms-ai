using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Update;

public sealed partial class UpdateTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<UpdateTenantCommandHandler> logger)
    : ICommandHandler<UpdateTenantCommand>
{
    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(TenantId.From(request.TenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var result = tenant.UpdateProfile(
            request.Name,
            request.PrimaryDomain,
            request.ContactEmail,
            request.DefaultLocale ?? tenant.DefaultLocale,
            request.DefaultTimeZone ?? tenant.DefaultTimeZone,
            clock.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        TenantUpdated(logger, request.TenantId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information,
        Message = "Tenant updated: {TenantId}")]
    private static partial void TenantUpdated(ILogger logger, Guid tenantId);
}
