using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingAttempt;

public sealed class GetSpeakingAttemptQueryHandler(ISpeakingAttemptRepository attempts, ICurrentUserService currentUser)
    : IQueryHandler<GetSpeakingAttemptQuery, SpeakingAttemptResponse>
{
    public async Task<Result<SpeakingAttemptResponse>> Handle(GetSpeakingAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts
            .GetByIdAsync(SpeakingAttemptId.From(request.AttemptId), cancellationToken)
            .ConfigureAwait(false);

        var isOwner = attempt is not null && attempt.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (attempt is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<SpeakingAttemptResponse>(SpeakingErrors.AttemptNotFound);
        }

        return Result.Success(CreateSpeakingAttemptCommandHandler.ToResponse(attempt));
    }
}
