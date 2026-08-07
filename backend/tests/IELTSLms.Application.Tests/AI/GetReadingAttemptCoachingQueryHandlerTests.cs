using System.Text.Json;
using IELTSLms.Application.AI.ReadingCoaching.Queries.GetReadingAttemptCoaching;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestReadingCoaching;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI;

/// <summary>Contract test per Sprint 8's testing section: attempt-review coaching must delegate to the
/// same underlying <c>RequestReadingCoachingCommand</c> (no second, parallel gateway path), and the
/// grounding text handed to it must be built strictly from the attempt's own passage/question/answer
/// data — the structural half of the "no hallucination" guarantee.</summary>
public sealed class GetReadingAttemptCoachingQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Fact]
    public async Task Attempt_not_found_or_not_owned_is_rejected()
    {
        var handler = new GetReadingAttemptCoachingQueryHandler(
            new FakeReadingAttemptRepository(), new FakeReadingPassageRepository(), new FakeReadingQuestionRepository(),
            new FakeSender());

        var result = await handler.Handle(
            new GetReadingAttemptCoachingQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_READING_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task An_unsubmitted_attempt_is_rejected_without_calling_the_sender()
    {
        var attempts = new FakeReadingAttemptRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var attempt = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, ReadingPassageId.From(Guid.NewGuid()), Now, 1800);
        attempts.Seed(attempt);
        var sender = new FakeSender();
        var handler = new GetReadingAttemptCoachingQueryHandler(
            attempts, new FakeReadingPassageRepository(), new FakeReadingQuestionRepository(), sender);

        var result = await handler.Handle(
            new GetReadingAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_ATTEMPT_NOT_SUBMITTED", result.Error.Code);
        Assert.Null(sender.LastRequest);
    }

    [Fact]
    public async Task An_attempt_with_no_wrong_answers_is_rejected_without_calling_the_sender()
    {
        var attempts = new FakeReadingAttemptRepository();
        var passages = new FakeReadingPassageRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "Ocean Currents", "Oceans move heat around the globe.",
            ReadingTrack.Academic, ReadingDifficulty.Intermediate, null, 1800, Now).Value;
        passages.Seed(passage);
        var attempt = ReadingAttempt.Start(ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, passage.Id, Now, 1800);
        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = Guid.NewGuid(), SubmittedAnswer = "b", IsCorrect = true } });
        attempt.Submit(answers, 1, 1, 9.0m, Now.AddMinutes(10));
        attempts.Seed(attempt);
        var sender = new FakeSender();
        var handler = new GetReadingAttemptCoachingQueryHandler(attempts, passages, new FakeReadingQuestionRepository(), sender);

        var result = await handler.Handle(
            new GetReadingAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_READING_ATTEMPT_NO_ERRORS", result.Error.Code);
        Assert.Null(sender.LastRequest);
    }

    [Fact]
    public async Task Wrong_answers_are_grounded_in_the_passage_and_delegated_to_RequestReadingCoachingCommand()
    {
        var attempts = new FakeReadingAttemptRepository();
        var passages = new FakeReadingPassageRepository();
        var questions = new FakeReadingQuestionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var passage = ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "Ocean Currents", "Oceans move heat around the globe.",
            ReadingTrack.Academic, ReadingDifficulty.Intermediate, null, 1800, Now).Value;
        passages.Seed(passage);

        var question = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, passage.Id, ReadingQuestionType.MultipleChoice,
            "What drives ocean currents?", Key("heat"), 0, Now).Value;
        questions.Seed(question);

        var attempt = ReadingAttempt.Start(ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, passage.Id, Now, 1800);
        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = question.Id.Value, SubmittedAnswer = "wind", IsCorrect = false } });
        attempt.Submit(answers, 0, 1, 1.0m, Now.AddMinutes(10));
        attempts.Seed(attempt);

        var sender = new FakeSender { NextResponse = Result.Success(new AiOperationAcceptedResponse(Guid.NewGuid(), "queued")) };
        var handler = new GetReadingAttemptCoachingQueryHandler(attempts, passages, questions, sender);

        var result = await handler.Handle(
            new GetReadingAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        var delegatedCommand = Assert.IsType<RequestReadingCoachingCommand>(sender.LastRequest);
        Assert.Equal(tenantId, delegatedCommand.TenantId);
        Assert.Equal(userId, delegatedCommand.UserId);
        Assert.Contains("Oceans move heat around the globe.", delegatedCommand.Text);
        Assert.Contains("What drives ocean currents?", delegatedCommand.Text);
        Assert.Contains("wind", delegatedCommand.Text);
        Assert.Contains("heat", delegatedCommand.Text);
    }
}
