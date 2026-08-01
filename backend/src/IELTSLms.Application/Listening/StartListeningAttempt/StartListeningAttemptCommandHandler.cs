using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.StartListeningAttempt;

public sealed class StartListeningAttemptCommandHandler(
    IListeningAudioRepository audios, IListeningQuestionRepository questions, IListeningAttemptRepository attempts,
    IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<StartListeningAttemptCommand, ListeningAttemptStartResponse>
{
    public async Task<Result<ListeningAttemptStartResponse>> Handle(StartListeningAttemptCommand request, CancellationToken cancellationToken)
    {
        var audioId = ListeningAudioId.From(request.AudioId);
        var audio = await audios.GetVisibleByIdAsync(request.TenantId, audioId, cancellationToken).ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<ListeningAttemptStartResponse>(ListeningErrors.AudioNotFound);
        }

        var audioQuestions = await questions.ListByAudioAsync(audioId, cancellationToken).ConfigureAwait(false);
        if (audioQuestions.Count == 0)
        {
            return Result.Failure<ListeningAttemptStartResponse>(ListeningErrors.NoQuestionsToAttempt);
        }

        var now = clock.UtcNow;
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(guids.NewGuid()), request.TenantId, request.UserId, audioId, request.Mode, now, audio.DurationSeconds);

        attempts.Add(attempt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var allowedReplays = ListeningReplayPolicy.AllowedReplays(request.Mode);
        return Result.Success(new ListeningAttemptStartResponse(
            attempt.Id.Value, audioId.Value, attempt.Mode.ToString(), attempt.StartedAtUtc, attempt.DeadlineAtUtc,
            allowedReplays == int.MaxValue ? -1 : allowedReplays));
    }
}
