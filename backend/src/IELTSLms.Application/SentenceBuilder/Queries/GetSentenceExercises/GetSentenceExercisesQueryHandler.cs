using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercises;

public sealed class GetSentenceExercisesQueryHandler(ISentenceExerciseRepository exercises)
    : IQueryHandler<GetSentenceExercisesQuery, PagedList<SentenceExerciseResponse>>
{
    public async Task<Result<PagedList<SentenceExerciseResponse>>> Handle(
        GetSentenceExercisesQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var items = await exercises
            .ListVisibleAsync(request.TenantId, request.Difficulty, skip, request.PageSize, cancellationToken)
            .ConfigureAwait(false);
        var total = await exercises
            .CountVisibleAsync(request.TenantId, request.Difficulty, cancellationToken)
            .ConfigureAwait(false);

        var page = PagedList<SentenceExerciseResponse>.Create(
            items.Select(CreateSentenceExerciseCommandHandler.ToResponse).ToArray(), request.Page, request.PageSize, total);

        return Result.Success(page);
    }
}
