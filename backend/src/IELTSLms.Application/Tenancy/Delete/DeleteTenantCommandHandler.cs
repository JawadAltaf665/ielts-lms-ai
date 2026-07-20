using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Tenancy;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Tenancy.Delete;

public sealed partial class DeleteTenantCommandHandler(
    ITenantRepository tenants,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<DeleteTenantCommandHandler> logger)
    : ICommandHandler<DeleteTenantCommand>
{
    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenants.GetByIdAsync(TenantId.From(request.TenantId), cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        tenant.SoftDelete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        TenantDeleted(logger, request.TenantId);
        return Result.Success();
    }

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information,
        Message = "Tenant soft-deleted: {TenantId}")]
    private static partial void TenantDeleted(ILogger logger, Guid tenantId);
}
