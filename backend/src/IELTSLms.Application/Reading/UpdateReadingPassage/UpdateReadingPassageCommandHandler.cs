using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.UpdateReadingPassage;

/// <remarks>
/// Concurrency relies on the natural EF/xmin optimistic-concurrency check (load, mutate, save within
/// one request) — same as Grammar's <c>UpdateGrammarTopicCommand</c> (Phase 6 Sprint 1). A client-supplied
/// <c>If-Match</c> check would need generic ETag middleware that doesn't exist anywhere in this codebase
/// yet; ponytail: add if a real staleness bug shows up.
/// </remarks>
public sealed class UpdateReadingPassageCommandHandler(IReadingPassageRepository passages, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UpdateReadingPassageCommand, ReadingPassageResponse>
{
    public async Task<Result<ReadingPassageResponse>> Handle(UpdateReadingPassageCommand request, CancellationToken cancellationToken)
    {
        var passage = await passages.GetVisibleByIdAsync(request.TenantId, ReadingPassageId.From(request.PassageId), cancellationToken)
            .ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<ReadingPassageResponse>(ReadingErrors.PassageNotFound);
        }

        if (passage.IsGlobal)
        {
            return Result.Failure<ReadingPassageResponse>(ReadingErrors.PassageReadOnly);
        }

        if (!string.Equals(passage.Title, request.Title, StringComparison.Ordinal)
            && await passages.TitleExistsAsync(passage.TenantId, request.Title, passage.Id, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<ReadingPassageResponse>(ReadingErrors.PassageTitleTaken);
        }

        var updateResult = passage.Update(
            request.Title, request.PassageText, request.Track, request.Difficulty, request.CategoryId, request.DurationSeconds, clock.UtcNow);
        if (updateResult.IsFailure)
        {
            return Result.Failure<ReadingPassageResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(CreateReadingPassageCommandHandler.ToResponse(passage));
    }
}
