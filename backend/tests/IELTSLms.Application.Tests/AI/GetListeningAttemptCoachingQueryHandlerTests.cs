using System.Text.Json;
using IELTSLms.Application.AI.ListeningCoaching.Queries.GetListeningAttemptCoaching;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestListeningCoaching;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI;

/// <summary>Mirrors <c>GetReadingAttemptCoachingQueryHandlerTests</c> exactly, grounded in the audio's
/// transcript instead of a passage.</summary>
public sealed class GetListeningAttemptCoachingQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    [Fact]
    public async Task Attempt_not_found_or_not_owned_is_rejected()
    {
        var handler = new GetListeningAttemptCoachingQueryHandler(
            new FakeListeningAttemptRepository(), new FakeListeningAudioRepository(), new FakeListeningQuestionRepository(),
            new FakeSender());

        var result = await handler.Handle(
            new GetListeningAttemptCoachingQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LISTENING_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task An_attempt_with_no_wrong_answers_is_rejected_without_calling_the_sender()
    {
        var attempts = new FakeListeningAttemptRepository();
        var audios = new FakeListeningAudioRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Campus Tour", ReadingTrack.General, FileAssetId.From(Guid.NewGuid()),
            null, 600, "The library closes at nine.", Now).Value;
        audios.Seed(audio);
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, audio.Id, ListeningMode.Exam, Now, 1800);
        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = Guid.NewGuid(), SubmittedAnswer = "b", IsCorrect = true } });
        attempt.Submit(answers, 1, 1, 9.0m, Now.AddMinutes(10));
        attempts.Seed(attempt);
        var sender = new FakeSender();
        var handler = new GetListeningAttemptCoachingQueryHandler(attempts, audios, new FakeListeningQuestionRepository(), sender);

        var result = await handler.Handle(
            new GetListeningAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LISTENING_ATTEMPT_NO_ERRORS", result.Error.Code);
        Assert.Null(sender.LastRequest);
    }

    [Fact]
    public async Task Wrong_answers_are_grounded_in_the_transcript_and_delegated_to_RequestListeningCoachingCommand()
    {
        var attempts = new FakeListeningAttemptRepository();
        var audios = new FakeListeningAudioRepository();
        var questions = new FakeListeningQuestionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Campus Tour", ReadingTrack.General, FileAssetId.From(Guid.NewGuid()),
            null, 600, "The library closes at nine.", Now).Value;
        audios.Seed(audio);

        var question = ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audio.Id, ReadingQuestionType.MultipleChoice,
            "When does the library close?", Key("nine"), 0, Now).Value;
        questions.Seed(question);

        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, audio.Id, ListeningMode.Exam, Now, 600);
        var answers = JsonSerializer.Serialize(new[] { new { QuestionId = question.Id.Value, SubmittedAnswer = "seven", IsCorrect = false } });
        attempt.Submit(answers, 0, 1, 1.0m, Now.AddMinutes(10));
        attempts.Seed(attempt);

        var sender = new FakeSender { NextResponse = Result.Success(new AiOperationAcceptedResponse(Guid.NewGuid(), "queued")) };
        var handler = new GetListeningAttemptCoachingQueryHandler(attempts, audios, questions, sender);

        var result = await handler.Handle(
            new GetListeningAttemptCoachingQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        var delegatedCommand = Assert.IsType<RequestListeningCoachingCommand>(sender.LastRequest);
        Assert.Contains("The library closes at nine.", delegatedCommand.Text);
        Assert.Contains("When does the library close?", delegatedCommand.Text);
        Assert.Contains("seven", delegatedCommand.Text);
        Assert.Contains("nine", delegatedCommand.Text);
    }
}
