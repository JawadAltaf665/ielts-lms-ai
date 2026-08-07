using IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.SentenceBuilder.Fakes;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder.Events;

namespace IELTSLms.Application.Tests.SentenceBuilder;

public sealed class SubmitSentenceAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static SentenceExercise SeedExercise(FakeSentenceExerciseRepository exercises, Guid tenantId)
    {
        var exercise = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), tenantId, "Write a concessive sentence.",
            """["although","would"]""", SentenceDifficulty.Intermediate, Now).Value;
        exercises.Seed(exercise);
        return exercise;
    }

    [Fact]
    public async Task Grades_the_submission_deterministically_and_returns_the_score()
    {
        var exercises = new FakeSentenceExerciseRepository();
        var attempts = new FakeSentenceAttemptRepository();
        var tenantId = Guid.NewGuid();
        var exercise = SeedExercise(exercises, tenantId);
        var handler = new SubmitSentenceAttemptCommandHandler(
            exercises, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new SubmitSentenceAttemptCommand(
                tenantId, Guid.NewGuid(), exercise.Id.Value, "Although it rained, she would go."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Score);
        Assert.Null(result.Value.AiResponseId);
        Assert.Null(result.Value.Feedback);
    }

    [Fact]
    public async Task Persisted_attempt_raises_SentenceAttemptSubmitted()
    {
        var exercises = new FakeSentenceExerciseRepository();
        var attempts = new FakeSentenceAttemptRepository();
        var tenantId = Guid.NewGuid();
        var exercise = SeedExercise(exercises, tenantId);
        var handler = new SubmitSentenceAttemptCommandHandler(
            exercises, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        await handler.Handle(
            new SubmitSentenceAttemptCommand(tenantId, Guid.NewGuid(), exercise.Id.Value, "no pattern here"),
            CancellationToken.None);

        var attempt = Assert.Single(attempts.Attempts);
        var raised = Assert.Single(attempt.DomainEvents);
        Assert.IsType<SentenceAttemptSubmittedDomainEvent>(raised);
    }

    [Fact]
    public async Task Missing_exercise_is_rejected()
    {
        var handler = new SubmitSentenceAttemptCommandHandler(
            new FakeSentenceExerciseRepository(), new FakeSentenceAttemptRepository(), new FakeUnitOfWork(),
            new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new SubmitSentenceAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "text"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SENTENCE_EXERCISE", result.Error.Code);
    }

    [Fact]
    public async Task Resubmitting_creates_an_independent_second_attempt_rather_than_mutating_the_first()
    {
        var exercises = new FakeSentenceExerciseRepository();
        var attempts = new FakeSentenceAttemptRepository();
        var tenantId = Guid.NewGuid();
        var exercise = SeedExercise(exercises, tenantId);
        var handler = new SubmitSentenceAttemptCommandHandler(
            exercises, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var command = new SubmitSentenceAttemptCommand(tenantId, Guid.NewGuid(), exercise.Id.Value, "Although it rained, she would go.");

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(2, attempts.Attempts.Count);
        Assert.NotEqual(first.Value.Id, second.Value.Id);
    }
}
