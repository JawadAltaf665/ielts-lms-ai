using IELTSLms.Application.Assessment.Commands.CompleteMockExamSection;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Assessment.Events;

namespace IELTSLms.Application.Tests.Assessment;

/// <summary>
/// Drives a full mock exam through all four sections (Sprint 8 Tier-1 "end-to-end" test) at the
/// Application tier with test doubles standing in for the AI-evaluated sections' latency, matching
/// this codebase's existing test-double convention (no Testcontainers/live DB in this repo's
/// Integration.Tests harness — see the fakes in <c>Assessment/Fakes</c>).
/// </summary>
public sealed class CompleteMockExamSectionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (FakeMockExamRepository MockExams, FakeMockExamResultRepository Results, MonthlyMockExam Exam, MockExamResult Result)
        SeedExamWithAllSectionsStarted(Guid tenantId, Guid userId)
    {
        var mockExams = new FakeMockExamRepository();
        var mockExamResults = new FakeMockExamResultRepository();
        var exam = MockExamFactory.Start(MockExamId.From(Guid.NewGuid()), tenantId, userId, Now);
        var result = MockExamResult.CreatePending(MockExamResultId.From(Guid.NewGuid()), tenantId, userId, exam.Id, Now);

        foreach (var section in Enum.GetValues<MockExamSectionType>())
        {
            exam.StartSection(section, Guid.NewGuid(), Now, Now.AddSeconds(3600));
        }

        mockExams.Seed(exam);
        mockExamResults.Seed(result);
        return (mockExams, mockExamResults, exam, result);
    }

    [Fact]
    public async Task Exam_finalizes_correctly_and_exactly_once_after_the_fourth_sections_evaluation()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var (mockExams, mockExamResults, exam, result) = SeedExamWithAllSectionsStarted(tenantId, userId);
        var handler = new CompleteMockExamSectionCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), new FakeClock(Now));

        var afterReading = await handler.Handle(
            new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, MockExamSectionType.Reading, 7.0m), CancellationToken.None);
        var afterListening = await handler.Handle(
            new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, MockExamSectionType.Listening, 7.0m), CancellationToken.None);
        var afterWriting = await handler.Handle(
            new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, MockExamSectionType.Writing, 6.5m), CancellationToken.None);

        Assert.True(afterReading.IsSuccess);
        Assert.True(afterListening.IsSuccess);
        Assert.True(afterWriting.IsSuccess);
        Assert.False(result.IsFinalized);
        Assert.Equal(MockExamState.InProgress, exam.State);

        var afterSpeaking = await handler.Handle(
            new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, MockExamSectionType.Speaking, 7.5m), CancellationToken.None);

        Assert.True(afterSpeaking.IsSuccess);
        Assert.True(result.IsFinalized);
        Assert.Equal(MockExamState.Completed, exam.State);
        Assert.Equal(BandCalculationService.CalculateOverallBand(7.0m, 7.0m, 6.5m, 7.5m), result.OverallBand);

        var raised = Assert.Single(exam.DomainEvents);
        Assert.IsType<MockExamCompletedDomainEvent>(raised);
    }

    [Fact]
    public async Task Re_delivering_the_same_sections_event_after_finalization_does_not_re_finalize()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var (mockExams, mockExamResults, exam, result) = SeedExamWithAllSectionsStarted(tenantId, userId);
        var handler = new CompleteMockExamSectionCommandHandler(mockExams, mockExamResults, new FakeUnitOfWork(), new FakeClock(Now));

        foreach (var (section, band) in new[]
                 {
                     (MockExamSectionType.Reading, 7.0m), (MockExamSectionType.Listening, 7.0m),
                     (MockExamSectionType.Writing, 7.0m), (MockExamSectionType.Speaking, 7.0m),
                 })
        {
            await handler.Handle(new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, section, band), CancellationToken.None);
        }

        Assert.True(result.IsFinalized);
        var finalizedAt = result.FinalizedAtUtc;

        // A redelivered event (e.g. an at-least-once notification retry) hits the already-finalized
        // result's own guard and fails cleanly rather than re-finalizing or raising a second event.
        var redelivered = await handler.Handle(
            new CompleteMockExamSectionCommand(tenantId, userId, exam.Id.Value, MockExamSectionType.Reading, 7.0m), CancellationToken.None);

        Assert.True(redelivered.IsFailure);
        Assert.Equal("CONFLICT_MOCK_EXAM_RESULT_ALREADY_FINALIZED", redelivered.Error.Code);
        Assert.Equal(finalizedAt, result.FinalizedAtUtc);
        Assert.Single(exam.DomainEvents); // still exactly one MockExamCompleted, not raised again
    }
}
