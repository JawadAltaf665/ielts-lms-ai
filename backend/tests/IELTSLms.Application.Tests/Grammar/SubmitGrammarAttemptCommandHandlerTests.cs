using System.Text.Json;
using IELTSLms.Application.Grammar.SubmitGrammarAttempt;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Grammar.Events;

namespace IELTSLms.Application.Tests.Grammar;

public sealed class SubmitGrammarAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    private static (Guid TenantId, GrammarTopicId TopicId, GrammarExerciseId ExerciseId, GrammarQuestionId Q1, GrammarQuestionId Q2)
        SeedExerciseWithTwoQuestions(FakeGrammarExerciseRepository exercises, FakeGrammarQuestionRepository questions)
    {
        var tenantId = Guid.NewGuid();
        var topicId = GrammarTopicId.From(Guid.NewGuid());
        var exercise = GrammarExercise.Create(
            GrammarExerciseId.From(Guid.NewGuid()), tenantId, topicId, "Drill 1", GrammarExerciseMode.Practice, 0, Now).Value;
        exercises.Seed(exercise);

        var q1 = GrammarQuestion.Create(
            GrammarQuestionId.From(Guid.NewGuid()), tenantId, exercise.Id, GrammarQuestionType.MultipleChoice, "Pick one", Key("b"), 0, Now).Value;
        var q2 = GrammarQuestion.Create(
            GrammarQuestionId.From(Guid.NewGuid()), tenantId, exercise.Id, GrammarQuestionType.FillBlank, "Fill it", Key("cat"), 1, Now).Value;
        questions.Seed(q1);
        questions.Seed(q2);

        return (tenantId, topicId, exercise.Id, q1.Id, q2.Id);
    }

    [Fact]
    public async Task Grades_each_answer_and_computes_the_raw_score()
    {
        var exercises = new FakeGrammarExerciseRepository();
        var questions = new FakeGrammarQuestionRepository();
        var attempts = new FakeGrammarAttemptRepository();
        var (tenantId, _, exerciseId, q1, q2) = SeedExerciseWithTwoQuestions(exercises, questions);
        var userId = Guid.NewGuid();

        var handler = new SubmitGrammarAttemptCommandHandler(
            exercises, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new SubmitGrammarAttemptCommand(
                tenantId, userId, exerciseId.Value,
                [new GrammarAnswerSubmission(q1.Value, "b"), new GrammarAnswerSubmission(q2.Value, "dog")]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.RawScore);
        Assert.Equal(2, result.Value.MaxScore);
        Assert.Contains(result.Value.Answers, a => a.QuestionId == q1.Value && a.IsCorrect);
        Assert.Contains(result.Value.Answers, a => a.QuestionId == q2.Value && !a.IsCorrect);
    }

    [Fact]
    public async Task Persisted_attempt_raises_GrammarCompleted_with_the_computed_score()
    {
        var exercises = new FakeGrammarExerciseRepository();
        var questions = new FakeGrammarQuestionRepository();
        var attempts = new FakeGrammarAttemptRepository();
        var (tenantId, topicId, exerciseId, q1, q2) = SeedExerciseWithTwoQuestions(exercises, questions);
        var userId = Guid.NewGuid();

        var handler = new SubmitGrammarAttemptCommandHandler(
            exercises, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        await handler.Handle(
            new SubmitGrammarAttemptCommand(
                tenantId, userId, exerciseId.Value,
                [new GrammarAnswerSubmission(q1.Value, "b"), new GrammarAnswerSubmission(q2.Value, "cat")]),
            CancellationToken.None);

        var attempt = Assert.Single(attempts.Attempts);
        var raised = Assert.Single(attempt.DomainEvents);
        var completed = Assert.IsType<GrammarCompletedDomainEvent>(raised);
        Assert.Equal(topicId.Value, completed.GrammarTopicId);
        Assert.Equal(2, completed.RawScore);
        Assert.Equal(2, completed.MaxScore);
    }

    [Fact]
    public async Task Missing_exercise_is_rejected()
    {
        var handler = new SubmitGrammarAttemptCommandHandler(
            new FakeGrammarExerciseRepository(), new FakeGrammarQuestionRepository(), new FakeGrammarAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new SubmitGrammarAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), [new GrammarAnswerSubmission(Guid.NewGuid(), "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_GRAMMAR_EXERCISE", result.Error.Code);
    }

    [Fact]
    public async Task Exercise_with_no_questions_is_rejected()
    {
        var exercises = new FakeGrammarExerciseRepository();
        var tenantId = Guid.NewGuid();
        var exercise = GrammarExercise.Create(
            GrammarExerciseId.From(Guid.NewGuid()), tenantId, GrammarTopicId.From(Guid.NewGuid()), "Empty", GrammarExerciseMode.Practice, 0, Now).Value;
        exercises.Seed(exercise);

        var handler = new SubmitGrammarAttemptCommandHandler(
            exercises, new FakeGrammarQuestionRepository(), new FakeGrammarAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new SubmitGrammarAttemptCommand(tenantId, Guid.NewGuid(), exercise.Id.Value, [new GrammarAnswerSubmission(Guid.NewGuid(), "b")]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_GRAMMAR_EXERCISE_EMPTY", result.Error.Code);
    }

    [Fact]
    public async Task Resubmitting_creates_an_independent_second_attempt_rather_than_mutating_the_first()
    {
        var exercises = new FakeGrammarExerciseRepository();
        var questions = new FakeGrammarQuestionRepository();
        var attempts = new FakeGrammarAttemptRepository();
        var (tenantId, _, exerciseId, q1, q2) = SeedExerciseWithTwoQuestions(exercises, questions);
        var userId = Guid.NewGuid();
        var handler = new SubmitGrammarAttemptCommandHandler(
            exercises, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var command = new SubmitGrammarAttemptCommand(
            tenantId, userId, exerciseId.Value, [new GrammarAnswerSubmission(q1.Value, "b"), new GrammarAnswerSubmission(q2.Value, "cat")]);

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(2, attempts.Attempts.Count);
        Assert.NotEqual(first.Value.Id, second.Value.Id);
        Assert.Equal(first.Value.RawScore, second.Value.RawScore);
    }
}
