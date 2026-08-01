using IELTSLms.Application.Assessment.Commands.AbandonMockExam;
using IELTSLms.Application.Assessment.Commands.StartMockExam;
using IELTSLms.Application.Tests.Assessment.Fakes;

namespace IELTSLms.Application.Tests.Assessment;

public sealed class AbandonMockExamCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Abandoning_mid_exam_releases_the_active_session_lock_so_a_new_one_can_start()
    {
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var clock = new FakeClock(Now);
        var startHandler = new StartMockExamCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), clock, new FakeGuidProvider());
        var abandonHandler = new AbandonMockExamCommandHandler(mockExams, new FakeUnitOfWork(), clock);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var started = await startHandler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);
        Assert.True(started.IsSuccess);

        var blocked = await startHandler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);
        Assert.True(blocked.IsFailure);
        Assert.Equal("BUSINESS_ACTIVE_EXAM_EXISTS", blocked.Error.Code);

        clock.UtcNow = Now.AddMinutes(5);
        var abandoned = await abandonHandler.Handle(new AbandonMockExamCommand(tenantId, userId, started.Value.Id), CancellationToken.None);
        Assert.True(abandoned.IsSuccess);
        Assert.Equal("Abandoned", abandoned.Value.State);

        var restarted = await startHandler.Handle(new StartMockExamCommand(tenantId, userId), CancellationToken.None);

        Assert.True(restarted.IsSuccess);
        Assert.Equal(2, mockExams.Exams.Count);
    }

    [Fact]
    public async Task Abandoning_someone_elses_exam_is_not_found_rather_than_forbidden()
    {
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var clock = new FakeClock(Now);
        var startHandler = new StartMockExamCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), clock, new FakeGuidProvider());
        var abandonHandler = new AbandonMockExamCommandHandler(mockExams, new FakeUnitOfWork(), clock);
        var tenantId = Guid.NewGuid();
        var started = await startHandler.Handle(new StartMockExamCommand(tenantId, Guid.NewGuid()), CancellationToken.None);

        var result = await abandonHandler.Handle(new AbandonMockExamCommand(tenantId, Guid.NewGuid(), started.Value.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_MOCK_EXAM", result.Error.Code);
    }
}
