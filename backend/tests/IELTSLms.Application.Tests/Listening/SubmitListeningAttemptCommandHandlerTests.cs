using IELTSLms.Application.Listening.SubmitListeningAttempt;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Listening;

namespace IELTSLms.Application.Tests.Listening;

public sealed class SubmitListeningAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, Guid UserId, ListeningQuestionId Q1, ListeningQuestionId Q2, ListeningAttempt Attempt)
        SeedStartedAttempt(FakeListeningQuestionRepository questions, FakeListeningAttemptRepository attempts, int durationSeconds = 1800)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var audioId = ListeningAudioId.From(Guid.NewGuid());

        var q1 = ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audioId, Domain.Reading.ReadingQuestionType.MultipleChoice, "Pick", "[\"b\"]", 0, Now).Value;
        var q2 = ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audioId, Domain.Reading.ReadingQuestionType.ShortAnswer, "Name it", "[\"cat\"]", 1, Now).Value;
        questions.Seed(q1);
        questions.Seed(q2);

        var attempt = ListeningAttempt.Start(ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, audioId, ListeningMode.Exam, Now, durationSeconds);
        attempts.Seed(attempt);

        return (tenantId, userId, q1.Id, q2.Id, attempt);
    }

    [Fact]
    public async Task Grades_each_answer_and_computes_the_band()
    {
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (_, userId, q1, q2, attempt) = SeedStartedAttempt(questions, attempts);
        var handler = new SubmitListeningAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(5)));

        var result = await handler.Handle(
            new SubmitListeningAttemptCommand(
                attempt.TenantId, userId, attempt.Id.Value,
                [new ListeningAnswerSubmission(q1.Value, "b"), new ListeningAnswerSubmission(q2.Value, "dog")]),
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
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (_, userId, q1, _, attempt) = SeedStartedAttempt(questions, attempts, durationSeconds: 60);
        var handler = new SubmitListeningAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddSeconds(61)));

        var result = await handler.Handle(
            new SubmitListeningAttemptCommand(attempt.TenantId, userId, attempt.Id.Value, [new ListeningAnswerSubmission(q1.Value, "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LISTENING_ATTEMPT_EXPIRED", result.Error.Code);
    }

    [Fact]
    public async Task Resubmitting_an_already_scored_attempt_returns_the_stored_result_without_rescoring()
    {
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (_, userId, q1, q2, attempt) = SeedStartedAttempt(questions, attempts);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SubmitListeningAttemptCommandHandler(attempts, questions, unitOfWork, new FakeClock(Now.AddMinutes(5)));
        var command = new SubmitListeningAttemptCommand(
            attempt.TenantId, userId, attempt.Id.Value,
            [new ListeningAnswerSubmission(q1.Value, "b"), new ListeningAnswerSubmission(q2.Value, "cat")]);

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.RawScore, second.Value.RawScore);
        Assert.Equal(first.Value.SubmittedAtUtc, second.Value.SubmittedAtUtc);
        Assert.Single(attempts.Attempts);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Another_users_attempt_is_not_found_rather_than_forbidden()
    {
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (_, _, q1, _, attempt) = SeedStartedAttempt(questions, attempts);
        var handler = new SubmitListeningAttemptCommandHandler(attempts, questions, new FakeUnitOfWork(), new FakeClock(Now.AddMinutes(5)));

        var result = await handler.Handle(
            new SubmitListeningAttemptCommand(attempt.TenantId, Guid.NewGuid(), attempt.Id.Value, [new ListeningAnswerSubmission(q1.Value, "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LISTENING_ATTEMPT", result.Error.Code);
    }
}
