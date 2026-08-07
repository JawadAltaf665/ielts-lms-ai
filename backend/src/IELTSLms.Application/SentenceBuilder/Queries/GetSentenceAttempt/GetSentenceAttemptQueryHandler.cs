using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceAttempt;

public sealed class GetSentenceAttemptQueryHandler(ISentenceAttemptRepository attempts)
    : IQueryHandler<GetSentenceAttemptQuery, SentenceAttemptResponse>
{
    public async Task<Result<SentenceAttemptResponse>> Handle(GetSentenceAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts
            .GetByIdAsync(SentenceAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<SentenceAttemptResponse>(SentenceBuilderErrors.AttemptNotFound);
        }

        return Result.Success(SubmitSentenceAttemptCommandHandler.ToResponse(attempt));
    }
}
