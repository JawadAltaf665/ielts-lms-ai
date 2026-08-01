using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.SubmitReadingAttempt;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.GetReadingAttempt;

public sealed class GetReadingAttemptQueryHandler(IReadingAttemptRepository attempts)
    : IQueryHandler<GetReadingAttemptQuery, ReadingAttemptResponse>
{
    public async Task<Result<ReadingAttemptResponse>> Handle(GetReadingAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ReadingAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<ReadingAttemptResponse>(ReadingErrors.AttemptNotFound);
        }

        return Result.Success(SubmitReadingAttemptCommandHandler.ToResponse(attempt));
    }
}
