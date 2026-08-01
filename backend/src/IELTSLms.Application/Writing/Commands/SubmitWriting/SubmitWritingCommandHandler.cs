using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Commands.SubmitWriting;

/// <summary>
/// Creates and submits a <see cref="WritingSubmission"/>, then kicks off the async AI evaluation
/// workflow directly (entitlement/quota check, <see cref="AiOperation"/> creation, job enqueue) rather
/// than via a <c>WritingSubmittedDomainEvent</c> handler — see that event's own remarks for why. Mirrors
/// <c>RequestGrammarCoachingCommandHandler</c>'s AI-kickoff shape, plus the submission bookkeeping.
/// </summary>
public sealed class SubmitWritingCommandHandler(
    IWritingTaskRepository tasks, IWritingSubmissionRepository submissions, IAiOperationRepository operations,
    IUnitOfWork unitOfWork, IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<SubmitWritingCommand, WritingSubmissionAcceptedResponse>
{
    public async Task<Result<WritingSubmissionAcceptedResponse>> Handle(
        SubmitWritingCommand request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetVisibleByIdAsync(request.TenantId, WritingTaskId.From(request.WritingTaskId), cancellationToken)
            .ConfigureAwait(false);
        if (task is null)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(WritingErrors.TaskNotFound);
        }

        var wordCount = CountWords(request.ResponseText);
        if (wordCount < task.MinWordCount || wordCount > task.MaxWordCount)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(WritingErrors.WordCountOutOfBounds);
        }

        var now = clock.UtcNow;
        var createResult = WritingSubmission.Create(
            WritingSubmissionId.From(guids.NewGuid()), request.TenantId, request.UserId, task.Id, request.ResponseText, now);
        if (createResult.IsFailure)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(createResult.Error);
        }

        var submission = createResult.Value;
        var submitResult = submission.Submit(wordCount, now);
        if (submitResult.IsFailure)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(submitResult.Error);
        }

        // INV-03: entitlement check precedes any AI call.
        var entitled = await entitlements
            .HasFeatureAsync(request.TenantId, AiFeatures.WritingEvaluationEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var quotaCheck = await quotas
            .CheckAsync(request.TenantId, AiFeatures.WritingEvaluationQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<WritingSubmissionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas
            .ConsumeAsync(request.TenantId, AiFeatures.WritingEvaluationQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return Result.Failure<WritingSubmissionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, "writing-evaluation", request.ResponseText, now);

        submissions.Add(submission);
        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var submissionId = submission.Id.Value;
        backgroundJobs.Enqueue<WritingEvaluationJob>(
            job => job.ExecuteAsync(tenantId, operationId, submissionId, CancellationToken.None));

        return Result.Success(new WritingSubmissionAcceptedResponse(submissionId, operationId, "queued"));
    }

    private static int CountWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}
