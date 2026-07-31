using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.GetListeningAudioStream;

public sealed class GetListeningAudioStreamQueryHandler(
    IListeningAudioRepository audios, IFileAssetRepository files, IFileStorage fileStorage, ITenantContextAccessor tenantContext, IClock clock)
    : IQueryHandler<GetListeningAudioStreamQuery, ListeningStreamUrlResponse>
{
    private static readonly TimeSpan UrlExpiry = TimeSpan.FromMinutes(15);

    public async Task<Result<ListeningStreamUrlResponse>> Handle(GetListeningAudioStreamQuery request, CancellationToken cancellationToken)
    {
        var audio = await audios.GetVisibleByIdAsync(request.TenantId, ListeningAudioId.From(request.AudioId), cancellationToken)
            .ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<ListeningStreamUrlResponse>(ListeningErrors.AudioNotFound);
        }

        // A global (catalog) audio's file may belong to whichever tenant originally seeded it; audio
        // visibility was already proven above, so this direct-by-id lookup bypasses the tenant filter
        // rather than re-deriving cross-tenant catalog rules for Files.
        using var bypass = tenantContext.BypassTenantFilter();
        var file = await files.GetByIdAsync(audio.FileId, cancellationToken).ConfigureAwait(false);
        if (file is null)
        {
            return Result.Failure<ListeningStreamUrlResponse>(ListeningErrors.AudioFileMissing);
        }

        var url = await fileStorage.GetSignedUrlAsync(file.StorageKey, UrlExpiry, cancellationToken).ConfigureAwait(false);
        return Result.Success(new ListeningStreamUrlResponse(url, clock.UtcNow.Add(UrlExpiry)));
    }
}
