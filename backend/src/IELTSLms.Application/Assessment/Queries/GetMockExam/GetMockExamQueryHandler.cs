using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Assessment;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Assessment.Queries.GetMockExam;

public sealed class GetMockExamQueryHandler(IMockExamRepository mockExams) : IQueryHandler<GetMockExamQuery, MockExamResponse>
{
    public async Task<Result<MockExamResponse>> Handle(GetMockExamQuery request, CancellationToken cancellationToken)
    {
        var mockExam = await mockExams.GetByIdAsync(MockExamId.From(request.MockExamId), cancellationToken).ConfigureAwait(false);
        if (mockExam is null || mockExam.UserId != request.UserId)
        {
            return Result.Failure<MockExamResponse>(MockExamErrors.NotFound);
        }

        return Result.Success(ToResponse(mockExam));
    }

    internal static MockExamResponse ToResponse(MonthlyMockExam mockExam)
    {
        var sections = Enum.GetValues<MockExamSectionType>()
            .Select(section => new MockExamSectionResponse(
                section.ToString(), mockExam.SectionAttemptId(section), mockExam.SectionStartedAt(section),
                SectionDeadline(mockExam, section), SectionCompletedAt(mockExam, section)))
            .ToArray();

        return new MockExamResponse(mockExam.Id.Value, mockExam.State.ToString(), mockExam.StartedAtUtc, mockExam.CompletedAtUtc, mockExam.AbandonedAtUtc, sections);
    }

    private static DateTimeOffset? SectionDeadline(MonthlyMockExam mockExam, MockExamSectionType section) => section switch
    {
        MockExamSectionType.Reading => mockExam.ReadingDeadlineAtUtc,
        MockExamSectionType.Listening => mockExam.ListeningDeadlineAtUtc,
        MockExamSectionType.Writing => mockExam.WritingDeadlineAtUtc,
        MockExamSectionType.Speaking => mockExam.SpeakingDeadlineAtUtc,
        _ => null,
    };

    private static DateTimeOffset? SectionCompletedAt(MonthlyMockExam mockExam, MockExamSectionType section) => section switch
    {
        MockExamSectionType.Reading => mockExam.ReadingCompletedAtUtc,
        MockExamSectionType.Listening => mockExam.ListeningCompletedAtUtc,
        MockExamSectionType.Writing => mockExam.WritingCompletedAtUtc,
        MockExamSectionType.Speaking => mockExam.SpeakingCompletedAtUtc,
        _ => null,
    };
}
