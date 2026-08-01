using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;

/// <summary>
/// Records the section's band, marks it complete, and — once all four are in — finalizes the
/// <c>MockExamResult</c> and completes the <c>MonthlyMockExam</c> in the same save. The exam and its
/// 1:1 result are treated as one transactional unit here (both owned by this same Assessment-context
/// command, not a genuine cross-bounded-context write) rather than two separate saves, since a
/// "completed exam with no finalized result" or vice versa would itself be an inconsistency.
/// </summary>
public sealed class CompleteMockExamSectionCommandHandler(
    IMockExamRepository mockExams, IMockExamResultRepository mockExamResults, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CompleteMockExamSectionCommand>
{
    public async Task<Result> Handle(CompleteMockExamSectionCommand request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Success();
        }

        var result = await mockExamResults.GetByMockExamIdAsync(mockExam.Id, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Result.Success();
        }

        var now = clock.UtcNow;

        var recordResult = result.RecordSectionBand(request.Section, request.Band, now);
        if (recordResult.IsFailure)
        {
            return recordResult;
        }

        var completeSectionResult = mockExam.CompleteSection(request.Section, now);
        if (completeSectionResult.IsFailure)
        {
            return completeSectionResult;
        }

        if (mockExam.AllSectionsCompleted && !result.IsFinalized)
        {
            var finalizeResult = result.Finalize(now);
            if (finalizeResult.IsFailure)
            {
                return finalizeResult;
            }

            var completeExamResult = mockExam.Complete(result.OverallBand!.Value, now);
            if (completeExamResult.IsFailure)
            {
                return completeExamResult;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
