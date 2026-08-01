using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;

public sealed class CreateSpeakingAttemptCommandHandler(
    ISpeakingAttemptRepository attempts, IFileAssetRepository files, IFileStorage fileStorage, IUnitOfWork unitOfWork,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateSpeakingAttemptCommand, SpeakingAttemptResponse>
{
    public async Task<Result<SpeakingAttemptResponse>> Handle(CreateSpeakingAttemptCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var fileId = FileAssetId.From(guids.NewGuid());
        var storageKey = $"speaking-attempt/{fileId.Value}{Path.GetExtension(request.FileName)}";

        var fileResult = FileAsset.Create(
            fileId, request.TenantId, request.UserId, storageKey, request.FileName, request.ContentType, request.SizeBytes,
            SpeakingAudioFilePolicy.AllowedContentTypes, SpeakingAudioFilePolicy.MaxSizeBytes, now);
        if (fileResult.IsFailure)
        {
            return Result.Failure<SpeakingAttemptResponse>(fileResult.Error);
        }

        var attemptResult = SpeakingAttempt.Create(
            SpeakingAttemptId.From(guids.NewGuid()), request.TenantId, request.UserId, SpeakingTaskId.From(request.SpeakingTaskId),
            fileId, request.DurationSeconds, now);
        if (attemptResult.IsFailure)
        {
            return Result.Failure<SpeakingAttemptResponse>(attemptResult.Error);
        }

        // All validation passes before any byte is written. ponytail: an upload that succeeds followed
        // by a failed SaveChanges leaves an orphaned object on disk; same accepted gap as
        // CreateListeningAudioCommandHandler — a background reconciliation sweep is the upgrade path.
        await fileStorage.UploadAsync(storageKey, request.FileContent, request.ContentType, cancellationToken).ConfigureAwait(false);

        files.Add(fileResult.Value);
        attempts.Add(attemptResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(attemptResult.Value));
    }

    internal static SpeakingAttemptResponse ToResponse(SpeakingAttempt attempt) => new(
        attempt.Id.Value, attempt.UserId, attempt.SpeakingTaskId.Value, attempt.DurationSeconds, attempt.TranscriptText,
        attempt.Status.ToString(), attempt.SubmittedAtUtc, attempt.SpeakingEvaluationId?.Value);
}
