using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.RegisterListeningReplay;

public sealed class RegisterListeningReplayCommandHandler(
    IListeningAttemptRepository attempts, IListeningAudioRepository audios, IFileAssetRepository files, IFileStorage fileStorage,
    ITenantContextAccessor tenantContext, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RegisterListeningReplayCommand, ListeningReplayResponse>
{
    private static readonly TimeSpan UrlExpiry = TimeSpan.FromMinutes(15);

    public async Task<Result<ListeningReplayResponse>> Handle(RegisterListeningReplayCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ListeningAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<ListeningReplayResponse>(ListeningErrors.AttemptNotFound);
        }

        var now = clock.UtcNow;
        if (attempt.IsExpired(now))
        {
            return Result.Failure<ListeningReplayResponse>(ListeningErrors.AttemptExpired);
        }

        // Resolve the file before spending a replay slot, so an infra lookup failure never silently
        // consumes the learner's cap. A global (catalog) audio's file may belong to whichever tenant
        // seeded it; visibility was already proven when the attempt was started, so this direct-by-id
        // lookup bypasses the tenant filter rather than re-deriving cross-tenant catalog rules.
        string storageKey;
        using (tenantContext.BypassTenantFilter())
        {
            var audio = await audios.GetByIdAsync(attempt.ListeningAudioId, cancellationToken).ConfigureAwait(false);
            var file = audio is null ? null : await files.GetByIdAsync(audio.FileId, cancellationToken).ConfigureAwait(false);
            if (file is null)
            {
                return Result.Failure<ListeningReplayResponse>(ListeningErrors.AudioFileMissing);
            }

            storageKey = file.StorageKey;
        }

        var replayResult = attempt.RegisterReplay(now);
        if (replayResult.IsFailure)
        {
            return Result.Failure<ListeningReplayResponse>(replayResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var url = await fileStorage.GetSignedUrlAsync(storageKey, UrlExpiry, cancellationToken).ConfigureAwait(false);
        return Result.Success(new ListeningReplayResponse(attempt.ReplayCount, url, now.Add(UrlExpiry)));
    }
}
