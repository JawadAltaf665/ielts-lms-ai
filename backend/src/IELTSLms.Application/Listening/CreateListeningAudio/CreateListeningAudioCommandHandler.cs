using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.CreateListeningAudio;

public sealed class CreateListeningAudioCommandHandler(
    IListeningAudioRepository audios, IFileAssetRepository files, IFileStorage fileStorage, IUnitOfWork unitOfWork,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateListeningAudioCommand, ListeningAudioResponse>
{
    public async Task<Result<ListeningAudioResponse>> Handle(CreateListeningAudioCommand request, CancellationToken cancellationToken)
    {
        if (await audios.TitleExistsAsync(request.TenantId, request.Title, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<ListeningAudioResponse>(ListeningErrors.AudioTitleTaken);
        }

        var now = clock.UtcNow;
        var fileId = FileAssetId.From(guids.NewGuid());
        var storageKey = $"listening-audio/{fileId.Value}{Path.GetExtension(request.FileName)}";

        var fileResult = FileAsset.Create(
            fileId, request.TenantId, request.UserId, storageKey, request.FileName, request.ContentType, request.SizeBytes,
            ListeningAudioFilePolicy.AllowedContentTypes, ListeningAudioFilePolicy.MaxSizeBytes, now);
        if (fileResult.IsFailure)
        {
            return Result.Failure<ListeningAudioResponse>(fileResult.Error);
        }

        var audioResult = ListeningAudio.Create(
            ListeningAudioId.From(guids.NewGuid()), request.TenantId, request.Title, request.Track, fileId, request.CategoryId,
            request.DurationSeconds, request.TranscriptText, now);
        if (audioResult.IsFailure)
        {
            return Result.Failure<ListeningAudioResponse>(audioResult.Error);
        }

        // All validation passes before any byte is written. ponytail: an upload that succeeds followed
        // by a failed SaveChanges leaves an orphaned object on disk; acceptable for this sprint — a
        // background reconciliation sweep is the upgrade path if that ever matters in practice.
        await fileStorage.UploadAsync(storageKey, request.FileContent, request.ContentType, cancellationToken).ConfigureAwait(false);

        files.Add(fileResult.Value);
        audios.Add(audioResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(audioResult.Value));
    }

    internal static ListeningAudioResponse ToResponse(ListeningAudio audio) => new(
        audio.Id.Value, audio.TenantId, audio.Title, audio.Track.ToString(), audio.CategoryId, audio.DurationSeconds, audio.IsGlobal);
}
