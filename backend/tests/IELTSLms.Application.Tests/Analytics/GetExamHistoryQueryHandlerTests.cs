using IELTSLms.Application.Analytics.Queries.GetExamHistory;
using IELTSLms.Application.Tests.Assessment.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Domain.Assessment;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Analytics;

public sealed class GetExamHistoryQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static GetExamHistoryQueryHandler CreateHandler(
        FakeWritingSubmissionRepository writing, FakeGrammarAttemptRepository grammar, FakeMockExamResultRepository mockExams) =>
        new(writing, new FakeSpeakingAttemptRepository(), new FakeReadingAttemptRepository(), new FakeListeningAttemptRepository(),
            grammar, new FakeVocabularyProgressRepository(), mockExams);

    private static MockExamResult SeedFinalizedMockExam(FakeMockExamResultRepository repo, Guid tenantId, Guid userId, DateTimeOffset createdAt)
    {
        var result = MockExamResult.CreatePending(
            MockExamResultId.From(Guid.NewGuid()), tenantId, userId, MockExamId.From(Guid.NewGuid()), createdAt);
        result.RecordSectionBand(MockExamSectionType.Reading, 6.0m, createdAt);
        result.RecordSectionBand(MockExamSectionType.Listening, 6.0m, createdAt);
        result.RecordSectionBand(MockExamSectionType.Writing, 6.0m, createdAt);
        result.RecordSectionBand(MockExamSectionType.Speaking, 6.0m, createdAt);
        result.Finalize(createdAt);
        repo.Seed(result);
        return result;
    }

    [Fact]
    public async Task Merges_items_from_multiple_skills_ordered_newest_first()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var writing = new FakeWritingSubmissionRepository();
        var submission = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "some response text", Now).Value;
        submission.Submit(4, Now);
        writing.Seed(submission);

        var grammar = new FakeGrammarAttemptRepository();
        var grammarAttempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, GrammarExerciseId.From(Guid.NewGuid()), Guid.NewGuid(),
            "[]", 8, 10, Now.AddMinutes(5));
        grammar.Seed(grammarAttempt);

        var mockExams = new FakeMockExamResultRepository();
        var result = SeedFinalizedMockExam(mockExams, tenantId, userId, Now.AddMinutes(10));

        var handler = CreateHandler(writing, grammar, mockExams);

        var page = await handler.Handle(new GetExamHistoryQuery(tenantId, userId, Cursor: null, Limit: 20), CancellationToken.None);

        Assert.True(page.IsSuccess);
        Assert.Equal(3, page.Value.Items.Count);
        Assert.Equal("mock-exam", page.Value.Items[0].Type);
        Assert.Equal(result.Id.Value, page.Value.Items[0].Id);
        Assert.Equal("grammar", page.Value.Items[1].Type);
        Assert.Equal("writing", page.Value.Items[2].Type);
        Assert.Null(page.Value.NextCursor);
    }

    [Fact]
    public async Task Cursor_pagination_returns_a_next_cursor_when_more_items_remain_and_continues_correctly()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockExams = new FakeMockExamResultRepository();
        var oldest = SeedFinalizedMockExam(mockExams, tenantId, userId, Now);
        var middle = SeedFinalizedMockExam(mockExams, tenantId, userId, Now.AddMinutes(1));
        var newest = SeedFinalizedMockExam(mockExams, tenantId, userId, Now.AddMinutes(2));
        var handler = CreateHandler(new FakeWritingSubmissionRepository(), new FakeGrammarAttemptRepository(), mockExams);

        var firstPage = await handler.Handle(
            new GetExamHistoryQuery(tenantId, userId, Cursor: null, Limit: 2), CancellationToken.None);

        Assert.True(firstPage.IsSuccess);
        Assert.Equal(2, firstPage.Value.Items.Count);
        Assert.Equal(newest.Id.Value, firstPage.Value.Items[0].Id);
        Assert.Equal(middle.Id.Value, firstPage.Value.Items[1].Id);
        Assert.NotNull(firstPage.Value.NextCursor);

        var secondPage = await handler.Handle(
            new GetExamHistoryQuery(tenantId, userId, firstPage.Value.NextCursor, Limit: 2), CancellationToken.None);

        Assert.True(secondPage.IsSuccess);
        var item = Assert.Single(secondPage.Value.Items);
        Assert.Equal(oldest.Id.Value, item.Id);
        Assert.Null(secondPage.Value.NextCursor);
    }

    [Fact]
    public async Task Draft_writing_submissions_are_excluded_from_history()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var writing = new FakeWritingSubmissionRepository();
        var draft = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, WritingTaskId.From(Guid.NewGuid()), "draft text", Now).Value;
        writing.Seed(draft);
        var handler = CreateHandler(writing, new FakeGrammarAttemptRepository(), new FakeMockExamResultRepository());

        var page = await handler.Handle(new GetExamHistoryQuery(tenantId, userId, Cursor: null, Limit: 20), CancellationToken.None);

        Assert.True(page.IsSuccess);
        Assert.Empty(page.Value.Items);
    }
}
