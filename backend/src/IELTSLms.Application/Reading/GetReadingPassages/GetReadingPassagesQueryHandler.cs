using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.GetReadingPassages;

public sealed class GetReadingPassagesQueryHandler(IReadingPassageRepository passages)
    : IQueryHandler<GetReadingPassagesQuery, PagedList<ReadingPassageResponse>>
{
    public async Task<Result<PagedList<ReadingPassageResponse>>> Handle(GetReadingPassagesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await passages.ListVisibleAsync(
            request.TenantId, request.Track, request.Difficulty, skip, request.PageSize, cancellationToken).ConfigureAwait(false);
        var total = await passages.CountVisibleAsync(request.TenantId, request.Track, request.Difficulty, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<ReadingPassageResponse>.Create(
            items.Select(CreateReadingPassageCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
