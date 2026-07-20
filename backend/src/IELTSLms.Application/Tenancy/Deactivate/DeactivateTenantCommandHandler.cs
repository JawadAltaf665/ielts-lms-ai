using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Deactivate;

public sealed partial class DeactivateTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<DeactivateTenantCommandHandler> logger)
    : ICommandHandler<DeactivateTenantCommand>
{
    public async Task<Result> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(TenantId.From(request.TenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var result = tenant.Deactivate(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        TenantDeactivated(logger, request.TenantId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information,
        Message = "Tenant deactivated: {TenantId}")]
    private static partial void TenantDeactivated(ILogger logger, Guid tenantId);
}
