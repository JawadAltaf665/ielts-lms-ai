using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Restore;

public sealed partial class RestoreTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<RestoreTenantCommandHandler> logger)
    : ICommandHandler<RestoreTenantCommand>
{
    public async Task<Result> Handle(RestoreTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdIncludingDeletedAsync(
            TenantId.From(request.TenantId), cancellationToken);

        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var result = tenant.Restore(clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        TenantRestored(logger, request.TenantId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 3005, Level = LogLevel.Information,
        Message = "Tenant restored: {TenantId}")]
    private static partial void TenantRestored(ILogger logger, Guid tenantId);
}
