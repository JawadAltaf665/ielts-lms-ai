using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;

public sealed class SubmitSentenceAttemptCommandHandler(
    ISentenceExerciseRepository exercises, ISentenceAttemptRepository attempts, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<SubmitSentenceAttemptCommand, SentenceAttemptResponse>
{
    public async Task<Result<SentenceAttemptResponse>> Handle(SubmitSentenceAttemptCommand request, CancellationToken cancellationToken)
    {
        var exerciseId = SentenceExerciseId.From(request.ExerciseId);
        var exercise = await exercises.GetVisibleByIdAsync(request.TenantId, exerciseId, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return Result.Failure<SentenceAttemptResponse>(SentenceBuilderErrors.ExerciseNotFound);
        }

        var score = SentencePatternMatcher.Score(exercise.TargetPatternJson, request.SubmittedText);
        var now = clock.UtcNow;

        var attempt = SentenceAttempt.Submit(
            SentenceAttemptId.From(guids.NewGuid()), request.TenantId, request.UserId, exerciseId, request.SubmittedText, score, now);

        attempts.Add(attempt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(attempt));
    }

    internal static SentenceAttemptResponse ToResponse(SentenceAttempt attempt) => new(
        attempt.Id.Value, attempt.SentenceExerciseId.Value, attempt.SubmittedText, attempt.Score,
        attempt.AiResponseId?.Value, attempt.Feedback, attempt.CreatedAtUtc);
}
