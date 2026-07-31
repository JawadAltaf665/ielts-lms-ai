using IELTSLms.Application.Reading.SubmitReadingAttempt;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading;

public sealed class SubmitReadingAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, Guid UserId, ReadingPassageId PassageId, ReadingQuestionId Q1, ReadingQuestionId Q2, ReadingAttempt Attempt)
        SeedStartedAttempt(FakeReadingQuestionRepository questions, FakeReadingAttemptRepository attempts, int durationSeconds = 1800)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var passageId = ReadingPassageId.From(Guid.NewGuid());

        var q1 = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passageId, ReadingQuestionType.MultipleChoice, "Pick", "[\"b\"]", 0, Now).Value;
        var q2 = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passageId, ReadingQuestionType.ShortAnswer, "Name it", "[\"cat\"]", 1, Now).Value;
        questions.Seed(q1);
        questions.Seed(q2);

        var attempt = ReadingAttempt.Start(ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, passageId, Now, durationSeconds);
        attempts.Seed(attempt);

        return (tenantId, userId, passageId, q1.Id, q2.Id, attempt);
    }

    [Fact]
    public async Task Grades_each_answer_and_computes_the_band()
    {
        var questions = new FakeReadingQuestionRepository();
        var attempts = new FakeReadingAttemptRepository();
        var (_, userId, _, q1, q2, attempt) = SeedStartedAttempt(questions, attempts);
        var handler = new SubmitReadingAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(5)));

        var result = await handler.Handle(
            new SubmitReadingAttemptCommand(
                attempt.TenantId, userId, attempt.Id.Value,
                [new ReadingAnswerSubmission(q1.Value, "b"), new ReadingAnswerSubmission(q2.Value, "dog")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.RawScore);
        Assert.Equal(2, result.Value.MaxScore);
        Assert.NotNull(result.Value.Band);
        Assert.Contains(result.Value.Answers!, a => a.QuestionId == q1.Value && a.IsCorrect);
        Assert.Contains(result.Value.Answers!, a => a.QuestionId == q2.Value && !a.IsCorrect);
    }

    [Fact]
    public async Task Submitting_after_the_server_deadline_is_rejected()
    {
        var questions = new FakeReadingQuestionRepository();
        var attempts = new FakeReadingAttemptRepository();
        var (_, userId, _, q1, _, attempt) = SeedStartedAttempt(questions, attempts, durationSeconds: 60);
        // Advance the fake clock past the deadline instead of a real Thread.Sleep.
        var handler = new SubmitReadingAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddSeconds(61)));

        var result = await handler.Handle(
            new SubmitReadingAttemptCommand(attempt.TenantId, userId, attempt.Id.Value, [new ReadingAnswerSubmission(q1.Value, "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_READING_ATTEMPT_EXPIRED", result.Error.Code);
    }

    [Fact]
    public async Task Resubmitting_an_already_scored_attempt_returns_the_stored_result_without_rescoring()
    {
        var questions = new FakeReadingQuestionRepository();
        var attempts = new FakeReadingAttemptRepository();
        var (_, userId, _, q1, q2, attempt) = SeedStartedAttempt(questions, attempts);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SubmitReadingAttemptCommandHandler(attempts, questions, unitOfWork, new FakeClock(Now.AddMinutes(5)));
        var command = new SubmitReadingAttemptCommand(
            attempt.TenantId, userId, attempt.Id.Value,
            [new ReadingAnswerSubmission(q1.Value, "b"), new ReadingAnswerSubmission(q2.Value, "cat")]);

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.RawScore, second.Value.RawScore);
        Assert.Equal(first.Value.SubmittedAtUtc, second.Value.SubmittedAtUtc);
        Assert.Single(attempts.Attempts);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount); // second call short-circuits before any save.
    }

    [Fact]
    public async Task Another_users_attempt_is_not_found_rather_than_forbidden()
    {
        var questions = new FakeReadingQuestionRepository();
        var attempts = new FakeReadingAttemptRepository();
        var (_, _, _, q1, _, attempt) = SeedStartedAttempt(questions, attempts);
        var handler = new SubmitReadingAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(5)));

        var result = await handler.Handle(
            new SubmitReadingAttemptCommand(attempt.TenantId, Guid.NewGuid(), attempt.Id.Value, [new ReadingAnswerSubmission(q1.Value, "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task Missing_attempt_is_rejected()
    {
        var handler = new SubmitReadingAttemptCommandHandler(
            new FakeReadingAttemptRepository(), new FakeReadingQuestionRepository(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(
            new SubmitReadingAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), [new ReadingAnswerSubmission(Guid.NewGuid(), "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_ATTEMPT", result.Error.Code);
    }
}
