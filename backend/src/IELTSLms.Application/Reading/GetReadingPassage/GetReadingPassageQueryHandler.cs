using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.GetReadingPassage;

public sealed class GetReadingPassageQueryHandler(IReadingPassageRepository passages, IReadingQuestionRepository questions)
    : IQueryHandler<GetReadingPassageQuery, ReadingPassageDetailResponse>
{
    public async Task<Result<ReadingPassageDetailResponse>> Handle(GetReadingPassageQuery request, CancellationToken cancellationToken)
    {
        var passageId = ReadingPassageId.From(request.PassageId);
        var passage = await passages.GetVisibleByIdAsync(request.TenantId, passageId, cancellationToken).ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<ReadingPassageDetailResponse>(ReadingErrors.PassageNotFound);
        }

        var passageQuestions = await questions.ListByPassageAsync(passageId, cancellationToken).ConfigureAwait(false);

        return Result.Success(new ReadingPassageDetailResponse(
            passage.Id.Value, passage.TenantId, passage.Title, passage.PassageText, passage.Track.ToString(), passage.Difficulty.ToString(),
            passage.CategoryId, passage.DurationSeconds, passage.IsGlobal,
            passageQuestions.Select(q => new ReadingQuestionSummaryResponse(q.Id.Value, q.Type.ToString(), q.Prompt, q.Sequence)).ToArray()));
    }
}
