using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.CreateReadingPassage;

public sealed class CreateReadingPassageCommandHandler(
    IReadingPassageRepository passages, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateReadingPassageCommand, ReadingPassageResponse>
{
    public async Task<Result<ReadingPassageResponse>> Handle(CreateReadingPassageCommand request, CancellationToken cancellationToken)
    {
        // Passages created through the tenant-facing API are always tenant-owned; global-catalog
        // passages (TenantId null) are seeded content, not authored here.
        if (await passages.TitleExistsAsync(request.TenantId, request.Title, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<ReadingPassageResponse>(ReadingErrors.PassageTitleTaken);
        }

        var now = clock.UtcNow;
        var createResult = ReadingPassage.Create(
            ReadingPassageId.From(guids.NewGuid()), request.TenantId, request.Title, request.PassageText, request.Track,
            request.Difficulty, request.CategoryId, request.DurationSeconds, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<ReadingPassageResponse>(createResult.Error);
        }

        var passage = createResult.Value;
        passages.Add(passage);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(passage));
    }

    internal static ReadingPassageResponse ToResponse(ReadingPassage passage) => new(
        passage.Id.Value, passage.TenantId, passage.Title, passage.PassageText, passage.Track.ToString(), passage.Difficulty.ToString(),
        passage.CategoryId, passage.DurationSeconds, passage.IsGlobal);
}
