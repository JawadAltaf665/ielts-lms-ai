using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.WeaknessDetection.Commands.RefreshWeaknessProfile;

public sealed class RefreshWeaknessProfileCommandHandler(
    IProgressTrackingRepository progressTracking, IAiOperationRepository operations, IUnitOfWork unitOfWork,
    IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs, IClock clock, IGuidProvider guids)
    : ICommandHandler<RefreshWeaknessProfileCommand, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        RefreshWeaknessProfileCommand request, CancellationToken cancellationToken)
    {
        // Business rule: below the minimum-data threshold, don't burn AI budget on a low-confidence
        // guess — fail clean before touching entitlement/quota/AiOperation at all.
        var rows = await progressTracking.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        if (!ProgressAnalysisService.IsSufficient(rows))
        {
            return Result.Failure<AiOperationAcceptedResponse>(LearningErrors.InsufficientData);
        }

        // INV-03: entitlement check precedes any AI call.
        var entitled = await entitlements
            .HasFeatureAsync(request.TenantId, AiFeatures.DailyLearningWorkflowEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var quotaCheck = await quotas
            .CheckAsync(request.TenantId, AiFeatures.DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas
            .ConsumeAsync(request.TenantId, AiFeatures.DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.ProgressAnalysis, string.Empty, now);

        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var userId = request.UserId;
        var operationId = operation.Id.Value;
        backgroundJobs.Enqueue<ProgressAnalysisJob>(job => job.ExecuteAsync(tenantId, operationId, userId, CancellationToken.None));

        return Result.Success(new AiOperationAcceptedResponse(operationId, "queued"));
    }
}
