using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.StartListeningAttempt;
using IELTSLms.Application.Reading.StartReadingAttempt;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Assessment.Commands.StartMockExamSection;

public sealed class StartMockExamSectionCommandHandler(IMockExamRepository mockExams, IUnitOfWork unitOfWork, IClock clock, ISender sender)
    : ICommandHandler<StartMockExamSectionCommand, MockExamSectionStartResponse>
{
    public async Task<Result<MockExamSectionStartResponse>> Handle(StartMockExamSectionCommand request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<MockExamSectionStartResponse>(MockExamErrors.NotFound);
        }

        var now = clock.UtcNow;
        Guid? attemptId = null;
        DateTimeOffset startedAtUtc;
        DateTimeOffset deadlineAtUtc;

        switch (request.Section)
        {
            case MockExamSectionType.Reading:
            {
                if (request.ContentId is not { } passageId)
                {
                    return Result.Failure<MockExamSectionStartResponse>(MockExamErrors.ContentIdRequired);
                }

                var started = await sender.Send(
                    new StartReadingAttemptCommand(request.TenantId, request.UserId, passageId), cancellationToken).ConfigureAwait(false);
                if (started.IsFailure)
                {
                    return Result.Failure<MockExamSectionStartResponse>(started.Error);
                }

                attemptId = started.Value.Id;
                startedAtUtc = started.Value.StartedAtUtc;
                deadlineAtUtc = started.Value.DeadlineAtUtc;
                break;
            }
            case MockExamSectionType.Listening:
            {
                if (request.ContentId is not { } audioId)
                {
                    return Result.Failure<MockExamSectionStartResponse>(MockExamErrors.ContentIdRequired);
                }

                var started = await sender.Send(
                    new StartListeningAttemptCommand(request.TenantId, request.UserId, audioId, ListeningMode.Exam), cancellationToken).ConfigureAwait(false);
                if (started.IsFailure)
                {
                    return Result.Failure<MockExamSectionStartResponse>(started.Error);
                }

                attemptId = started.Value.Id;
                startedAtUtc = started.Value.StartedAtUtc;
                deadlineAtUtc = started.Value.DeadlineAtUtc;
                break;
            }
            case MockExamSectionType.Writing:
                startedAtUtc = now;
                deadlineAtUtc = now.AddSeconds(MockExamSectionDurations.WritingSeconds);
                break;
            case MockExamSectionType.Speaking:
                startedAtUtc = now;
                deadlineAtUtc = now.AddSeconds(MockExamSectionDurations.SpeakingSeconds);
                break;
            default:
                return Result.Failure<MockExamSectionStartResponse>(Error.Validation(
                    "VALIDATION_MOCK_EXAM_SECTION_INVALID", $"'{request.Section}' is not a valid mock exam section."));
        }

        var startResult = mockExam.StartSection(request.Section, attemptId, startedAtUtc, deadlineAtUtc);
        if (startResult.IsFailure)
        {
            return Result.Failure<MockExamSectionStartResponse>(startResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new MockExamSectionStartResponse(mockExam.Id.Value, request.Section.ToString(), attemptId, startedAtUtc, deadlineAtUtc));
    }
}
