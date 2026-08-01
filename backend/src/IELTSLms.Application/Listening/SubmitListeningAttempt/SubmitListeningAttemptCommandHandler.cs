using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.SubmitListeningAttempt;

public sealed class SubmitListeningAttemptCommandHandler(
    IListeningAttemptRepository attempts, IListeningQuestionRepository questions, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<SubmitListeningAttemptCommand, ListeningAttemptResponse>
{
    public async Task<Result<ListeningAttemptResponse>> Handle(SubmitListeningAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ListeningAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<ListeningAttemptResponse>(ListeningErrors.AttemptNotFound);
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
            return Result.Failure<ListeningAttemptResponse>(ListeningErrors.AttemptExpired);
        }

        var audioQuestions = await questions.ListByAudioAsync(attempt.ListeningAudioId, cancellationToken).ConfigureAwait(false);
        var submittedByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.Answer);

        var graded = audioQuestions
            .Select(question =>
            {
                var submitted = submittedByQuestion.GetValueOrDefault(question.Id.Value);
                return new ListeningAttemptAnswerResponse(question.Id.Value, submitted, question.Grade(submitted));
            })
            .ToArray();

        var rawScore = graded.Count(a => a.IsCorrect);
        var maxScore = audioQuestions.Count;

        // Reuses Reading's band table (Phase 6 Sprint 4) — same 0-40 raw-score scale (Sprint 5
        // acceptance criteria: "no duplicated logic").
        var band = ReadingBandCalculator.ToBand(rawScore, maxScore);

        var submitResult = attempt.Submit(JsonSerializer.Serialize(graded), rawScore, maxScore, band, now);
        if (submitResult.IsFailure)
        {
            return Result.Failure<ListeningAttemptResponse>(submitResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(attempt, graded));
    }

    internal static ListeningAttemptResponse ToResponse(ListeningAttempt attempt, IReadOnlyList<ListeningAttemptAnswerResponse>? answers = null)
    {
        var resolvedAnswers = answers ?? (attempt.AnswersJson is null
            ? null
            : JsonSerializer.Deserialize<ListeningAttemptAnswerResponse[]>(attempt.AnswersJson));

        return new ListeningAttemptResponse(
            attempt.Id.Value, attempt.ListeningAudioId.Value, attempt.Mode.ToString(), attempt.StartedAtUtc, attempt.DeadlineAtUtc,
            attempt.SubmittedAtUtc, attempt.RawScore, attempt.MaxScore, attempt.Band, attempt.ReplayCount, resolvedAnswers);
    }
}
