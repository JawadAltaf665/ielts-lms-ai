using IELTSLms.Application.Reading.StartReadingAttempt;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading;

public sealed class StartReadingAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static ReadingPassage SeedPassage(FakeReadingPassageRepository passages, Guid tenantId, int durationSeconds = 1800)
    {
        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "Passage", "Text", ReadingTrack.Academic, ReadingDifficulty.Beginner, null,
            durationSeconds, Now).Value;
        passages.Seed(passage);
        return passage;
    }

    [Fact]
    public async Task Computes_the_deadline_on_the_server_from_the_passages_configured_duration()
    {
        var passages = new FakeReadingPassageRepository();
        var questions = new FakeReadingQuestionRepository();
        var attempts = new FakeReadingAttemptRepository();
        var tenantId = Guid.NewGuid();
        var passage = SeedPassage(passages, tenantId, durationSeconds: 2400);
        questions.Seed(ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passage.Id, ReadingQuestionType.ShortAnswer, "Q1", "[\"a\"]", 0, Now).Value);

        var handler = new StartReadingAttemptCommandHandler(
            passages, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new StartReadingAttemptCommand(tenantId, Guid.NewGuid(), passage.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, result.Value.StartedAtUtc);
        Assert.Equal(Now.AddSeconds(2400), result.Value.DeadlineAtUtc);
        Assert.Single(attempts.Attempts);
    }

    [Fact]
    public async Task Missing_passage_is_rejected()
    {
        var handler = new StartReadingAttemptCommandHandler(
            new FakeReadingPassageRepository(), new FakeReadingQuestionRepository(), new FakeReadingAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new StartReadingAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_PASSAGE", result.Error.Code);
    }

    [Fact]
    public async Task Passage_with_no_questions_cannot_be_started()
    {
        var passages = new FakeReadingPassageRepository();
        var tenantId = Guid.NewGuid();
        var passage = SeedPassage(passages, tenantId);
        var handler = new StartReadingAttemptCommandHandler(
            passages, new FakeReadingQuestionRepository(), new FakeReadingAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new StartReadingAttemptCommand(tenantId, Guid.NewGuid(), passage.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_PASSAGE_EMPTY", result.Error.Code);
    }

    [Fact]
    public async Task Cross_tenant_passage_is_not_found_rather_than_forbidden()
    {
        var passages = new FakeReadingPassageRepository();
        var passage = SeedPassage(passages, Guid.NewGuid());
        var handler = new StartReadingAttemptCommandHandler(
            passages, new FakeReadingQuestionRepository(), new FakeReadingAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new StartReadingAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), passage.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_PASSAGE", result.Error.Code);
    }
}
