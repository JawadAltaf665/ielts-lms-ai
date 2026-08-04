using System.Text.Json;
using IELTSLms.Application.AI.GrammarCoaching.Queries.GetGrammarAttemptCoaching;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI;

/// <summary>Contract test per Sprint 7's testing section: attempt-review coaching must delegate to the
/// same underlying <c>RequestGrammarCoachingCommand</c> (no second, parallel gateway path).</summary>
public sealed class GetGrammarAttemptCoachingQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Fact]
    public async Task Attempt_not_found_or_not_owned_is_rejected()
    {
        var handler = new GetGrammarAttemptCoachingQueryHandler(
            new FakeGrammarAttemptRepository(), new FakeGrammarQuestionRepository(), new FakeSender());

        var result = await handler.Handle(
            new GetGrammarAttemptCoachingQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_GRAMMAR_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task An_attempt_with_no_wrong_answers_is_rejected_without_calling_the_sender()
    {
        var attempts = new FakeGrammarAttemptRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exerciseId = GrammarExerciseId.From(Guid.NewGuid());
        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = Guid.NewGuid(), SubmittedAnswer = "b", IsCorrect = true } });
        var attempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, exerciseId, Guid.NewGuid(), answers, 1, 1, Now);
        attempts.Seed(attempt);
        var sender = new FakeSender();
        var handler = new GetGrammarAttemptCoachingQueryHandler(attempts, new FakeGrammarQuestionRepository(), sender);

        var result = await handler.Handle(
            new GetGrammarAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_GRAMMAR_ATTEMPT_NO_ERRORS", result.Error.Code);
        Assert.Null(sender.LastRequest);
    }

    [Fact]
    public async Task Wrong_answers_are_assembled_and_delegated_to_RequestGrammarCoachingCommand()
    {
        var attempts = new FakeGrammarAttemptRepository();
        var questions = new FakeGrammarQuestionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exerciseId = GrammarExerciseId.From(Guid.NewGuid());
        var question = GrammarQuestion.Create(
            GrammarQuestionId.From(Guid.NewGuid()), tenantId, exerciseId, GrammarQuestionType.MultipleChoice,
            "He ___ to school.", Key("goes"), 0, Now).Value;
        questions.Seed(question);

        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = question.Id.Value, SubmittedAnswer = "go", IsCorrect = false } });
        var attempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, exerciseId, Guid.NewGuid(), answers, 0, 1, Now);
        attempts.Seed(attempt);

        var sender = new FakeSender { NextResponse = Result.Success(new AiOperationAcceptedResponse(Guid.NewGuid(), "queued")) };
        var handler = new GetGrammarAttemptCoachingQueryHandler(attempts, questions, sender);

        var result = await handler.Handle(
            new GetGrammarAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        var delegatedCommand = Assert.IsType<RequestGrammarCoachingCommand>(sender.LastRequest);
        Assert.Equal(tenantId, delegatedCommand.TenantId);
        Assert.Equal(userId, delegatedCommand.UserId);
        Assert.Contains("He ___ to school.", delegatedCommand.Text);
        Assert.Contains("go", delegatedCommand.Text);
        Assert.Contains("goes", delegatedCommand.Text);
    }
}
