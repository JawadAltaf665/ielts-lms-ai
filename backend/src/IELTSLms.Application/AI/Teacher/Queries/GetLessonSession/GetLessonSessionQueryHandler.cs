using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Conversations;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.Teacher.Queries.GetLessonSession;

public sealed class GetLessonSessionQueryHandler(ILessonSessionRepository lessonSessions, IConversationMessageRepository messages)
    : IQueryHandler<GetLessonSessionQuery, LessonSessionDetailResponse>
{
    public async Task<Result<LessonSessionDetailResponse>> Handle(GetLessonSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await lessonSessions.GetByIdAsync(LessonSessionId.From(request.LessonSessionId), cancellationToken)
            .ConfigureAwait(false);
        if (session is null || session.TenantId != request.TenantId || session.UserId != request.UserId)
        {
            return Result.Failure<LessonSessionDetailResponse>(AiErrors.LessonSessionNotFound);
        }

        var history = await messages
            .ListSinceSequenceAsync(request.TenantId, session.ConversationId, afterSequence: 0, cancellationToken)
            .ConfigureAwait(false);

        var detail = new LessonSessionDetailResponse(
            LessonSessionResponseMapper.ToResponse(session),
            history.Select(ConversationMessageResponseMapper.ToResponse).ToArray());

        return Result.Success(detail);
    }
}
