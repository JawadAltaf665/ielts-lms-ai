using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Commands.SubmitMockExam;

public sealed class SubmitMockExamCommandHandler(
    IMockExamRepository mockExams, IMockExamResultRepository mockExamResults, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<SubmitMockExamCommand, MockExamSubmitResponse>
{
    public async Task<Result<MockExamSubmitResponse>> Handle(SubmitMockExamCommand request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<MockExamSubmitResponse>(MockExamErrors.NotFound);
        }

        var result = await mockExamResults.GetByMockExamIdAsync(mockExam.Id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Result.Failure<MockExamSubmitResponse>(MockExamErrors.ResultNotFound);
        }

        // Already finalized (e.g. the event-driven path beat this call to it) — idempotent success.
        if (result.IsFinalized)
        {
            return Result.Success(new MockExamSubmitResponse(mockExam.Id.Value, mockExam.State.ToString(), true, result.OverallBand));
        }

        if (!mockExam.AllSectionsCompleted || !result.HasAllSectionBands)
        {
            return Result.Failure<MockExamSubmitResponse>(MockExamErrors.SectionsIncomplete);
        }

        var now = clock.UtcNow;

        var finalizeResult = result.Finalize(now);
        if (finalizeResult.IsFailure)
        {
            return Result.Failure<MockExamSubmitResponse>(finalizeResult.Error);
        }

        var completeResult = mockExam.Complete(result.OverallBand!.Value, now);
        if (completeResult.IsFailure)
        {
            return Result.Failure<MockExamSubmitResponse>(completeResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new MockExamSubmitResponse(mockExam.Id.Value, mockExam.State.ToString(), true, result.OverallBand));
    }
}
