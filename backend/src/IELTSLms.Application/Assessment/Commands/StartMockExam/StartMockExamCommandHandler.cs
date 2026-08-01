using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Exceptions;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Commands.StartMockExam;

public sealed class StartMockExamCommandHandler(
    IMockExamRepository mockExams, IMockExamResultRepository mockExamResults, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<StartMockExamCommand, MockExamStartResponse>
{
    public async Task<Result<MockExamStartResponse>> Handle(StartMockExamCommand request, CancellationToken cancellationToken)
    {
        var existing = await mockExams.GetActiveInProgressByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result.Failure<MockExamStartResponse>(MockExamErrors.ActiveExamExists);
        }

        var now = clock.UtcNow;
        var mockExamId = MockExamId.From(guids.NewGuid());
        var mockExam = MockExamFactory.Start(mockExamId, request.TenantId, request.UserId, now);
        var result = MockExamResult.CreatePending(MockExamResultId.From(guids.NewGuid()), request.TenantId, request.UserId, mockExamId, now);

        mockExams.Add(mockExam);
        mockExamResults.Add(result);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (UniqueConstraintViolationException)
        {
            // Two simultaneous StartMockExam calls raced past the pre-check above; the DB's partial
            // unique index (tenant_id, user_id) WHERE state = 'InProgress' is the real arbiter — the
            // loser gets the same clean business conflict rather than a raw 500 (Sprint 8 Tier-1 test).
            return Result.Failure<MockExamStartResponse>(MockExamErrors.ActiveExamExists);
        }

        return Result.Success(new MockExamStartResponse(mockExam.Id.Value, mockExam.State.ToString(), mockExam.StartedAtUtc));
    }
}
