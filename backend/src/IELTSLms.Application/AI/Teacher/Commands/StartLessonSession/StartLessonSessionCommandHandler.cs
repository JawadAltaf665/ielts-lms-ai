using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Exceptions;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Teacher.Commands.StartLessonSession;

public sealed class StartLessonSessionCommandHandler(
    ILessonSessionRepository lessonSessions, IConversationRepository conversations, IAiOperationRepository operations,
    IUnitOfWork unitOfWork, IEntitlementService entitlements, IQuotaService quotas, IBackgroundJobService backgroundJobs,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<StartLessonSessionCommand, LessonSessionAcceptedResponse>
{
    public async Task<Result<LessonSessionAcceptedResponse>> Handle(StartLessonSessionCommand request, CancellationToken cancellationToken)
    {
        var entitled = await entitlements.HasFeatureAsync(request.TenantId, AiFeatures.AiTeacher, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return Result.Failure<LessonSessionAcceptedResponse>(AiErrors.FeatureNotEntitled);
        }

        var existing = await lessonSessions
            .GetActiveByUserAndLessonAsync(request.TenantId, request.UserId, request.LessonId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return Result.Failure<LessonSessionAcceptedResponse>(AiErrors.ActiveLessonSessionExists);
        }

        var quotaCheck = await quotas.CheckAsync(request.TenantId, AiFeatures.AiTeacherQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return Result.Failure<LessonSessionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var consumeResult = await quotas.ConsumeAsync(request.TenantId, AiFeatures.AiTeacherQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (consumeResult.IsFailure)
        {
            return Result.Failure<LessonSessionAcceptedResponse>(AiErrors.QuotaExceeded);
        }

        var now = clock.UtcNow;
        var conversation = Conversation.Create(
            ConversationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTeacher, now).Value;
        var session = LessonSession.Start(
            LessonSessionId.From(guids.NewGuid()), request.TenantId, request.UserId, request.LessonId, conversation.Id, now);
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, AiFeatures.AiTeacher, request.Topic, now);

        conversations.Add(conversation);
        lessonSessions.Add(session);
        operations.Add(operation);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (UniqueConstraintViolationException)
        {
            // Two simultaneous StartLessonSession calls raced past the pre-check above; the DB's
            // partial unique index (tenant_id, user_id, lesson_id) WHERE current_step <> 'Completed' is
            // the real arbiter, mirroring StartMockExamCommandHandler (Phase 6 Sprint 8).
            return Result.Failure<LessonSessionAcceptedResponse>(AiErrors.ActiveLessonSessionExists);
        }

        var tenantId = request.TenantId;
        var operationId = operation.Id.Value;
        var lessonSessionId = session.Id.Value;
        backgroundJobs.Enqueue<TeacherLessonStepJob>(
            job => job.ExecuteAsync(tenantId, operationId, lessonSessionId, isSessionStart: true, CancellationToken.None));

        return Result.Success(new LessonSessionAcceptedResponse(lessonSessionId, operationId, "queued"));
    }
}
