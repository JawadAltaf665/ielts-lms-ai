using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Queries.GetMockExamResult;

public sealed class GetMockExamResultQueryHandler(IMockExamRepository mockExams, IMockExamResultRepository mockExamResults)
    : IQueryHandler<GetMockExamResultQuery, MockExamResultResponse>
{
    public async Task<Result<MockExamResultResponse>> Handle(GetMockExamResultQuery request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<MockExamResultResponse>(MockExamErrors.NotFound);
        }

        var result = await mockExamResults.GetByMockExamIdAsync(mockExam.Id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Result.Failure<MockExamResultResponse>(MockExamErrors.ResultNotFound);
        }

        return Result.Success(new MockExamResultResponse(
            result.Id.Value, result.MockExamId.Value, result.IsFinalized ? "finalized" : "pending",
            result.ReadingBand, result.ListeningBand, result.WritingBand, result.SpeakingBand, result.OverallBand, result.FinalizedAtUtc));
    }
}
