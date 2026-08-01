using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Commands.SubmitSpeakingAttempt;

public sealed class SubmitSpeakingAttemptCommandHandler(
    ISpeakingAttemptRepository attempts, IAiOperationRepository operations, IUnitOfWork unitOfWork,
    IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs, IClock clock, IGuidProvider guids)
    : ICommandHandler<SubmitSpeakingAttemptCommand, SpeakingAttemptAcceptedResponse>
{
    public async Task<Result<SpeakingAttemptAcceptedResponse>> Handle(
        SubmitSpeakingAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attempts
            .GetByIdAsync(SpeakingAttemptId.From(request.AttemptId), cancellationToken)
            .ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(SpeakingErrors.AttemptNotFound);
        }

        if (attempt.Status != SpeakingAttemptStatus.Recorded)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(SpeakingErrors.AttemptNotRecorded);
        }

        // Minimum-duration business rule: rejected before it fails deep in the STT pipeline (sprint requirement).
        if (attempt.DurationSeconds < SpeakingAudioFilePolicy.MinDurationSeconds)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(SpeakingErrors.AttemptTooShort);
        }

        var now = clock.UtcNow;
        var submitResult = attempt.Submit(now);
        if (submitResult.IsFailure)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(submitResult.Error);
        }

        // INV-03-equivalent: entitlement check precedes any AI call.
        var entitled = await entitlements
            .HasFeatureAsync(request.TenantId, AiFeatures.SpeakingEvaluationEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var quotaCheck = await quotas
            .CheckAsync(request.TenantId, AiFeatures.SpeakingEvaluationQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<SpeakingAttemptAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas
            .ConsumeAsync(request.TenantId, AiFeatures.SpeakingEvaluationQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return Result.Failure<SpeakingAttemptAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        // No learner text exists yet at this point (transcription hasn't run) — InputText is unused by
        // the job, which loads the attempt by id the same way WritingEvaluationJob loads its submission.
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, "speaking-evaluation", string.Empty, now);

        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var attemptId = attempt.Id.Value;
        backgroundJobs.Enqueue<SpeakingEvaluationJob>(
            job => job.ExecuteAsync(tenantId, operationId, attemptId, CancellationToken.None));

        return Result.Success(new SpeakingAttemptAcceptedResponse(attemptId, operationId, "queued"));
    }
}
