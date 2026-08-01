using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.CreateListeningAudio;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.GetListeningAudios;

public sealed class GetListeningAudiosQueryHandler(IListeningAudioRepository audios)
    : IQueryHandler<GetListeningAudiosQuery, PagedList<ListeningAudioResponse>>
{
    public async Task<Result<PagedList<ListeningAudioResponse>>> Handle(GetListeningAudiosQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await audios.ListVisibleAsync(request.TenantId, request.Track, skip, request.PageSize, cancellationToken)
            .ConfigureAwait(false);
        var total = await audios.CountVisibleAsync(request.TenantId, request.Track, cancellationToken).ConfigureAwait(false);

        var page = PagedList<ListeningAudioResponse>.Create(
            items.Select(CreateListeningAudioCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
