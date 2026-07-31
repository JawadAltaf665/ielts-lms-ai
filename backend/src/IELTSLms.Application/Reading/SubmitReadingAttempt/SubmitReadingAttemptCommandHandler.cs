using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.SubmitReadingAttempt;

public sealed class SubmitReadingAttemptCommandHandler(
    IReadingAttemptRepository attempts, IReadingQuestionRepository questions, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<SubmitReadingAttemptCommand, ReadingAttemptResponse>
{
    public async Task<Result<ReadingAttemptResponse>> Handle(SubmitReadingAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ReadingAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<ReadingAttemptResponse>(ReadingErrors.AttemptNotFound);
        }

        // Idempotent scoring (ERD §4.3): resubmitting an already-scored attempt returns the stored
        // result rather than rescoring — never rely on the client to avoid a duplicate submit.
        if (attempt.IsSubmitted)
        {
            return Result.Success(ToResponse(attempt));
        }

        var now = clock.UtcNow;
        if (attempt.IsExpired(now))
        {
            return Result.Failure<ReadingAttemptResponse>(ReadingErrors.AttemptExpired);
        }

        var passageQuestions = await questions.ListByPassageAsync(attempt.ReadingPassageId, cancellationToken).ConfigureAwait(false);
        var submittedByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.Answer);

        var graded = passageQuestions
            .Select(question =>
            {
                var submitted = submittedByQuestion.GetValueOrDefault(question.Id.Value);
                return new ReadingAttemptAnswerResponse(question.Id.Value, submitted, question.Grade(submitted));
            })
            .ToArray();

        var rawScore = graded.Count(a => a.IsCorrect);
        var maxScore = passageQuestions.Count;
        var band = ReadingBandCalculator.ToBand(rawScore, maxScore);

        var submitResult = attempt.Submit(JsonSerializer.Serialize(graded), rawScore, maxScore, band, now);
        if (submitResult.IsFailure)
        {
            return Result.Failure<ReadingAttemptResponse>(submitResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new ReadingAttemptResponse(
            attempt.Id.Value, attempt.ReadingPassageId.Value, attempt.StartedAtUtc, attempt.DeadlineAtUtc, attempt.SubmittedAtUtc,
            attempt.RawScore, attempt.MaxScore, attempt.Band, graded));
    }

    internal static ReadingAttemptResponse ToResponse(ReadingAttempt attempt)
    {
        var answers = attempt.AnswersJson is null
            ? null
            : JsonSerializer.Deserialize<ReadingAttemptAnswerResponse[]>(attempt.AnswersJson);

        return new ReadingAttemptResponse(
            attempt.Id.Value, attempt.ReadingPassageId.Value, attempt.StartedAtUtc, attempt.DeadlineAtUtc, attempt.SubmittedAtUtc,
            attempt.RawScore, attempt.MaxScore, attempt.Band, answers);
    }
}
