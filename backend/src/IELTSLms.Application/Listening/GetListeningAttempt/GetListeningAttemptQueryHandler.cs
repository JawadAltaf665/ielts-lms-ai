using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.SubmitListeningAttempt;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.GetListeningAttempt;

public sealed class GetListeningAttemptQueryHandler(IListeningAttemptRepository attempts)
    : IQueryHandler<GetListeningAttemptQuery, ListeningAttemptResponse>
{
    public async Task<Result<ListeningAttemptResponse>> Handle(GetListeningAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ListeningAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<ListeningAttemptResponse>(ListeningErrors.AttemptNotFound);
        }

        return Result.Success(SubmitListeningAttemptCommandHandler.ToResponse(attempt));
    }
}
