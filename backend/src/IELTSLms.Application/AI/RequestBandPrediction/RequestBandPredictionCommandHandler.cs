using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.RequestBandPrediction;

public sealed class RequestBandPredictionCommandHandler(
    IBandPredictionRepository predictions, IAiOperationRepository operations, IUnitOfWork unitOfWork,
    IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs, IClock clock, IGuidProvider guids)
    : ICommandHandler<RequestBandPredictionCommand, BandPredictionAcceptedResponse>
{
    public async Task<Result<BandPredictionAcceptedResponse>> Handle(
        RequestBandPredictionCommand request, CancellationToken cancellationToken)
    {
        // INV-03: entitlement check precedes any AI call.
        var entitled = await entitlements
            .HasFeatureAsync(request.TenantId, AiFeatures.BandPredictorEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<BandPredictionAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var quotaCheck = await quotas
            .CheckAsync(request.TenantId, AiFeatures.BandPredictorQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<BandPredictionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas
            .ConsumeAsync(request.TenantId, AiFeatures.BandPredictorQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return Result.Failure<BandPredictionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, "band-predictor", string.Empty, now);
        var prediction = BandPrediction.Enqueue(
            BandPredictionId.From(guids.NewGuid()), request.TenantId, request.UserId, operation.Id, now);

        operations.Add(operation);
        predictions.Add(prediction);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var predictionId = prediction.Id.Value;
        backgroundJobs.Enqueue<BandPredictionJob>(
            job => job.ExecuteAsync(tenantId, operationId, predictionId, CancellationToken.None));

        return Result.Success(new BandPredictionAcceptedResponse(predictionId, operationId, "queued"));
    }
}
