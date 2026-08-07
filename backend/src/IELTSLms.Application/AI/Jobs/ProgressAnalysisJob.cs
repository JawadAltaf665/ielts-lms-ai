using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning.WeaknessDetection;
using IELTSLms.Domain.AI;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.AI.Jobs;

/// <summary>
/// AiOperation-tracked wrapper around <see cref="ProgressAnalysisService"/> for the learner-triggered
/// force-refresh endpoint (Sprint 9, <c>POST /users/{userId}/weakness-profile/refresh</c>) — gives the
/// client an operation resource to poll (API Design Standards §14.2), same shape as
/// <c>ReadingCoachingJob</c>. The scheduled/lazy-fallback paths call
/// <see cref="ProgressAnalysisService"/> directly instead (no one is polling those).
/// </summary>
public sealed partial class ProgressAnalysisJob(
    IAiOperationRepository operations, ProgressAnalysisService progressAnalysis, IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContextAccessor, IClock clock, ILogger<ProgressAnalysisJob> logger)
{
    public async Task ExecuteAsync(Guid tenantId, Guid operationId, Guid userId, CancellationToken cancellationToken = default)
    {
        tenantContextAccessor.SetTenant(tenantId);

        var operation = await operations.GetByIdAsync(AiOperationId.From(operationId), cancellationToken).ConfigureAwait(false);
        if (operation is null)
        {
            OperationNotFound(logger, operationId);
            return;
        }

        // Idempotency guard: Hangfire's global automatic-retry policy may re-run this job after a
        // mid-flight crash. A terminal operation has already been fully persisted.
        if (operation.Status is AiOperationStatus.Succeeded or AiOperationStatus.Failed)
        {
            return;
        }

        var now = clock.UtcNow;
        operation.MarkProcessing(now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var result = await progressAnalysis.ComputeAsync(tenantId, userId, now, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            operation.MarkFailed(result.Error.Code, result.Error.Message, now);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        // result.Value is null when the minimum-data-threshold gate wasn't met — the pre-check in
        // RefreshWeaknessProfileCommandHandler should have already caught this, but the operation still
        // needs a terminal, non-error status for the poller (small race window between the handler's
        // check and this job actually running is expected, not a bug).
        operation.MarkSucceeded(result.Value?.AiResponseId, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "AI operation {OperationId} not found; the enqueued job has nothing to execute.")]
    private static partial void OperationNotFound(ILogger logger, Guid operationId);
}
