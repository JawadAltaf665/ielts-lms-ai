using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Activate;

public sealed partial class ActivateTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<ActivateTenantCommandHandler> logger)
    : ICommandHandler<ActivateTenantCommand>
{
    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(TenantId.From(request.TenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var result = tenant.Activate(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        TenantActivated(logger, request.TenantId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information,
        Message = "Tenant activated: {TenantId}")]
    private static partial void TenantActivated(ILogger logger, Guid tenantId);
}
