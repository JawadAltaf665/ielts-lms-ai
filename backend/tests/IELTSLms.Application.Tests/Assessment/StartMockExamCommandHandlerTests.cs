using IELTSLms.Application.Assessment.Commands.StartMockExam;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Domain.Assessment;

namespace IELTSLms.Application.Tests.Assessment;

public sealed class StartMockExamCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_new_session_with_no_section_started_when_none_is_active()
    {
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var handler = new StartMockExamCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("InProgress", result.Value.State);
        Assert.Equal(Now, result.Value.StartedAtUtc);
        Assert.Single(mockExams.Exams);
        Assert.Single(mockExamResults.Results);
        Assert.Equal(mockExams.Exams[0].Id.Value, mockExamResults.Results[0].MockExamId.Value);
    }

    [Fact]
    public async Task Second_call_for_the_same_user_while_the_first_is_still_in_progress_gets_a_business_conflict()
    {
        // Simulates "two simultaneous StartMockExam calls for the same user" (Sprint 8 Tier-1 test):
        // both handler invocations share the same repository instance, so the second call's pre-check
        // sees the first call's already-added InProgress session - the same guard a DB race would hit
        // via the partial unique index (see ApplicationDbContext's UniqueConstraintViolationException
        // translation, which this fake-repository test cannot exercise without a live database).
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var handler = new StartMockExamCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var first = await handler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);
        var second = await handler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsFailure);
        Assert.Equal("BUSINESS_ACTIVE_EXAM_EXISTS", second.Error.Code);
        Assert.Single(mockExams.Exams); // exactly one session was created
    }

    [Fact]
    public async Task Different_users_can_each_have_their_own_active_session()
    {
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var handler = new StartMockExamCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var first = await handler.Handle(new StartMockExamCommand(tenantId, Guid.NewGuid()), CancellationToken.None);
        var second = await handler.Handle(new StartMockExamCommand(tenantId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, mockExams.Exams.Count);
    }

    [Fact]
    public async Task A_completed_prior_session_does_not_block_starting_a_new_one()
    {
        var mockExams = new FakeMockExamRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var priorExam = MockExamFactory.Start(MockExamId.From(Guid.NewGuid()), tenantId, userId, Now.AddDays(-30));
        priorExam.Abandon(Now.AddDays(-30));
        mockExams.Seed(priorExam);

        var handler = new StartMockExamCommandHandler(
            mockExams, new FakeMockExamResultRepository(), new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, mockExams.Exams.Count);
    }
}
