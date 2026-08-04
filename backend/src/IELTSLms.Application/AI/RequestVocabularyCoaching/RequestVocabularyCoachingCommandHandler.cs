using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.RequestVocabularyCoaching;

public sealed class RequestVocabularyCoachingCommandHandler(
    IAiOperationRepository operations, IUnitOfWork unitOfWork, IEntitlementService entitlements,
    IQuotaService quotas, IBackgroundJobService backgroundJobs, IClock clock, IGuidProvider guids)
    : ICommandHandler<RequestVocabularyCoachingCommand, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        RequestVocabularyCoachingCommand request, CancellationToken cancellationToken)
    {
        // INV-03: entitlement check precedes any AI call.
        var entitled = await entitlements
            .HasFeatureAsync(request.TenantId, AiFeatures.VocabularyCoachEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        // Quota check must short-circuit before any provider call is enqueued, not just log.
        var quotaCheck = await quotas
            .CheckAsync(request.TenantId, AiFeatures.VocabularyCoachQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas
            .ConsumeAsync(request.TenantId, AiFeatures.VocabularyCoachQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            // Lost the race against a concurrent request for the last slot in the period.
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var inputText = ComposeInputText(request.Word, request.Topic, request.Difficulty);
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.VocabularyCoach, inputText, now);

        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var operationId = operation.Id.Value;
        var tenantId = request.TenantId;
        backgroundJobs.Enqueue<VocabularyCoachingJob>(job => job.ExecuteAsync(tenantId, operationId, CancellationToken.None));

        return Result.Success(new AiOperationAcceptedResponse(operationId, "queued"));
    }

    private static string ComposeInputText(string word, string? topic, string? difficulty)
    {
        var text = $"Word: {word}";
        if (!string.IsNullOrWhiteSpace(topic))
        {
            text += $"\nTopic: {topic}";
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            text += $"\nDifficulty: {difficulty}";
        }

        return text;
    }
}
