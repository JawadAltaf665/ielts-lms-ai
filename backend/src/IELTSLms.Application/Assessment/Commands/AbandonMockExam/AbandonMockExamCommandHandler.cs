using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Commands.AbandonMockExam;

public sealed class AbandonMockExamCommandHandler(IMockExamRepository mockExams, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<AbandonMockExamCommand, MockExamAbandonResponse>
{
    public async Task<Result<MockExamAbandonResponse>> Handle(AbandonMockExamCommand request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<MockExamAbandonResponse>(MockExamErrors.NotFound);
        }

        var now = clock.UtcNow;
        var abandonResult = mockExam.Abandon(now);
        if (abandonResult.IsFailure)
        {
            return Result.Failure<MockExamAbandonResponse>(abandonResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new MockExamAbandonResponse(mockExam.Id.Value, mockExam.State.ToString(), mockExam.AbandonedAtUtc!.Value));
    }
}
