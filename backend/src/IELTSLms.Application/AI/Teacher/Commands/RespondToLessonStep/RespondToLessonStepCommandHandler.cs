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

namespace IELTSLms.Application.AI.Teacher.Commands.RespondToLessonStep;

public sealed class RespondToLessonStepCommandHandler(
    ILessonSessionRepository lessonSessions, IAiOperationRepository operations, IUnitOfWork unitOfWork,
    IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs, IClock clock, IGuidProvider guids)
    : ICommandHandler<RespondToLessonStepCommand, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(RespondToLessonStepCommand request, CancellationToken cancellationToken)
    {
        var entitled = await entitlements.HasFeatureAsync(request.TenantId, AiFeatures.AiTeacher, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var session = await lessonSessions.GetByIdAsync(LessonSessionId.From(request.LessonSessionId), cancellationToken)
            .ConfigureAwait(false);
        if (session is null || session.TenantId != request.TenantId || session.UserId != request.UserId)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.LessonSessionNotFound);
        }

        if (session.CurrentStep == LessonStep.Completed)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.LessonSessionCompleted);
        }

        var quotaCheck = await quotas.CheckAsync(request.TenantId, AiFeatures.AiTeacherQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas.ConsumeAsync(request.TenantId, AiFeatures.AiTeacherQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            return Result.Failure<AiOperationAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTeacher, request.Message, now);

        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var lessonSessionId = request.LessonSessionId;
        backgroundJobs.Enqueue<TeacherLessonStepJob>(
            job => job.ExecuteAsync(tenantId, operationId, lessonSessionId, isSessionStart: false, CancellationToken.None));

        return Result.Success(new AiOperationAcceptedResponse(operationId, "queued"));
    }
}
