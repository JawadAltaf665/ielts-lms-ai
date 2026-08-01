using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.StartReadingAttempt;

public sealed class StartReadingAttemptCommandHandler(
    IReadingPassageRepository passages, IReadingQuestionRepository questions, IReadingAttemptRepository attempts,
    IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<StartReadingAttemptCommand, ReadingAttemptStartResponse>
{
    public async Task<Result<ReadingAttemptStartResponse>> Handle(StartReadingAttemptCommand request, CancellationToken cancellationToken)
    {
        var passageId = ReadingPassageId.From(request.PassageId);
        var passage = await passages.GetVisibleByIdAsync(request.TenantId, passageId, cancellationToken).ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<ReadingAttemptStartResponse>(ReadingErrors.PassageNotFound);
        }

        var passageQuestions = await questions.ListByPassageAsync(passageId, cancellationToken).ConfigureAwait(false);
        if (passageQuestions.Count == 0)
        {
            return Result.Failure<ReadingAttemptStartResponse>(ReadingErrors.NoQuestionsToAttempt);
        }

        var now = clock.UtcNow;
        var attempt = ReadingAttempt.Start(
            ReadingAttemptId.From(guids.NewGuid()), request.TenantId, request.UserId, passageId, now, passage.DurationSeconds);

        attempts.Add(attempt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new ReadingAttemptStartResponse(attempt.Id.Value, passageId.Value, attempt.StartedAtUtc, attempt.DeadlineAtUtc));
    }
}
