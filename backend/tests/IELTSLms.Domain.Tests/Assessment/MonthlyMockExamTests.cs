using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Assessment.Events;

namespace IELTSLms.Domain.Tests.Assessment;

public sealed class MonthlyMockExamTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static MonthlyMockExam StartExam() =>
        MockExamFactory.Start(MockExamId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), Now);

    [Fact]
    public void Start_creates_an_in_progress_session_with_no_section_started()
    {
        var exam = StartExam();

        Assert.Equal(MockExamState.InProgress, exam.State);
        Assert.Equal(Now, exam.StartedAtUtc);
        Assert.False(exam.AllSectionsCompleted);
        Assert.All(Enum.GetValues<MockExamSectionType>(), section => Assert.Null(exam.SectionStartedAt(section)));
    }

    [Fact]
    public void StartSection_stamps_the_server_owned_timer_and_attempt_id_for_Reading()
    {
        var exam = StartExam();
        var attemptId = Guid.NewGuid();
        var deadline = Now.AddSeconds(3600);

        var result = exam.StartSection(MockExamSectionType.Reading, attemptId, Now, deadline);

        Assert.True(result.IsSuccess);
        Assert.Equal(attemptId, exam.ReadingAttemptId);
        Assert.Equal(Now, exam.ReadingStartedAtUtc);
        Assert.Equal(deadline, exam.ReadingDeadlineAtUtc);
    }

    [Fact]
    public void StartSection_twice_on_the_same_section_fails_rather_than_restarting_the_timer()
    {
        var exam = StartExam();
        exam.StartSection(MockExamSectionType.Writing, null, Now, Now.AddSeconds(3600));

        var second = exam.StartSection(MockExamSectionType.Writing, null, Now.AddMinutes(5), Now.AddMinutes(5).AddSeconds(3600));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_MOCK_EXAM_SECTION_ALREADY_STARTED", second.Error.Code);
        Assert.Equal(Now, exam.WritingStartedAtUtc);
    }

    [Fact]
    public void LinkSectionAttempt_attaches_the_writing_submission_id_once_the_section_was_started()
    {
        var exam = StartExam();
        exam.StartSection(MockExamSectionType.Writing, null, Now, Now.AddSeconds(3600));
        var submissionId = Guid.NewGuid();

        var result = exam.LinkSectionAttempt(MockExamSectionType.Writing, submissionId, Now.AddMinutes(10));

        Assert.True(result.IsSuccess);
        Assert.Equal(submissionId, exam.WritingSubmissionId);
    }

    [Fact]
    public void LinkSectionAttempt_fails_when_the_section_was_never_started()
    {
        var exam = StartExam();

        var result = exam.LinkSectionAttempt(MockExamSectionType.Speaking, Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_MOCK_EXAM_SECTION_NOT_STARTED", result.Error.Code);
    }

    [Fact]
    public void CompleteSection_is_idempotent()
    {
        var exam = StartExam();
        exam.StartSection(MockExamSectionType.Reading, Guid.NewGuid(), Now, Now.AddSeconds(3600));
        exam.CompleteSection(MockExamSectionType.Reading, Now.AddMinutes(30));

        var second = exam.CompleteSection(MockExamSectionType.Reading, Now.AddMinutes(45));

        Assert.True(second.IsSuccess);
        Assert.Equal(Now.AddMinutes(30), exam.ReadingCompletedAtUtc);
    }

    [Fact]
    public void Complete_fails_while_a_section_is_still_outstanding()
    {
        var exam = StartExam();
        exam.StartSection(MockExamSectionType.Reading, Guid.NewGuid(), Now, Now.AddSeconds(3600));
        exam.CompleteSection(MockExamSectionType.Reading, Now.AddMinutes(30));

        var result = exam.Complete(7.0m, Now.AddMinutes(31));

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_MOCK_EXAM_SECTIONS_INCOMPLETE", result.Error.Code);
    }

    [Fact]
    public void Complete_transitions_to_completed_and_raises_MockExamCompleted_once_all_four_sections_are_done()
    {
        var exam = StartExam();
        foreach (var section in Enum.GetValues<MockExamSectionType>())
        {
            exam.StartSection(section, Guid.NewGuid(), Now, Now.AddSeconds(3600));
            exam.CompleteSection(section, Now.AddMinutes(30));
        }

        var result = exam.Complete(7.0m, Now.AddMinutes(31));

        Assert.True(result.IsSuccess);
        Assert.Equal(MockExamState.Completed, exam.State);
        Assert.Equal(Now.AddMinutes(31), exam.CompletedAtUtc);
        var raised = Assert.Single(exam.DomainEvents);
        var completed = Assert.IsType<MockExamCompletedDomainEvent>(raised);
        Assert.Equal(7.0m, completed.OverallBand);
        Assert.Equal(exam.Id.Value, completed.MockExamId);
    }

    [Fact]
    public void Abandon_releases_the_single_active_session_lock()
    {
        var exam = StartExam();

        var result = exam.Abandon(Now.AddMinutes(5));

        Assert.True(result.IsSuccess);
        Assert.Equal(MockExamState.Abandoned, exam.State);
        Assert.Equal(Now.AddMinutes(5), exam.AbandonedAtUtc);
    }

    [Fact]
    public void Abandon_a_second_time_fails_rather_than_re_abandoning()
    {
        var exam = StartExam();
        exam.Abandon(Now);

        var second = exam.Abandon(Now.AddMinutes(1));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_MOCK_EXAM_NOT_IN_PROGRESS", second.Error.Code);
    }
}
