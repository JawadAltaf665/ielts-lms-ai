using IELTSLms.Application.Listening.StartListeningAttempt;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Listening;

public sealed class StartListeningAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static ListeningAudio SeedAudio(FakeListeningAudioRepository audios, Guid tenantId, int durationSeconds = 600)
    {
        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Section 1", ReadingTrack.Academic, FileAssetId.From(Guid.NewGuid()), null,
            durationSeconds, null, Now).Value;
        audios.Seed(audio);
        return audio;
    }

    [Fact]
    public async Task Computes_the_deadline_on_the_server_and_reports_the_exam_mode_cap_as_zero()
    {
        var audios = new FakeListeningAudioRepository();
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var tenantId = Guid.NewGuid();
        var audio = SeedAudio(audios, tenantId, durationSeconds: 900);
        questions.Seed(ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audio.Id, ReadingQuestionType.ShortAnswer, "Q1", "[\"a\"]", 0, Now).Value);

        var handler = new StartListeningAttemptCommandHandler(
            audios, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new StartListeningAttemptCommand(tenantId, Guid.NewGuid(), audio.Id.Value, ListeningMode.Exam), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, result.Value.StartedAtUtc);
        Assert.Equal(Now.AddSeconds(900), result.Value.DeadlineAtUtc);
        Assert.Equal(0, result.Value.AllowedReplays);
        Assert.Single(attempts.Attempts);
    }

    [Fact]
    public async Task Reports_the_learning_mode_cap_as_unlimited()
    {
        var audios = new FakeListeningAudioRepository();
        var questions = new FakeListeningQuestionRepository();
        var attempts = new FakeListeningAttemptRepository();
        var tenantId = Guid.NewGuid();
        var audio = SeedAudio(audios, tenantId);
        questions.Seed(ListeningQuestion.Create(
            ListeningQuestionId.From(Guid.NewGuid()), tenantId, audio.Id, ReadingQuestionType.ShortAnswer, "Q1", "[\"a\"]", 0, Now).Value);

        var handler = new StartListeningAttemptCommandHandler(
            audios, questions, attempts, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new StartListeningAttemptCommand(tenantId, Guid.NewGuid(), audio.Id.Value, ListeningMode.Learning), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(-1, result.Value.AllowedReplays);
    }

    [Fact]
    public async Task Missing_audio_is_rejected()
    {
        var handler = new StartListeningAttemptCommandHandler(
            new FakeListeningAudioRepository(), new FakeListeningQuestionRepository(), new FakeListeningAttemptRepository(),
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new StartListeningAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ListeningMode.Exam), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LISTENING_AUDIO", result.Error.Code);
    }

    [Fact]
    public async Task Audio_with_no_questions_cannot_be_started()
    {
        var audios = new FakeListeningAudioRepository();
        var tenantId = Guid.NewGuid();
        var audio = SeedAudio(audios, tenantId);
        var handler = new StartListeningAttemptCommandHandler(
            audios, new FakeListeningQuestionRepository(), new FakeListeningAttemptRepository(), new FakeUnitOfWork(), new FakeClock(Now),
            new FakeGuidProvider());

        var result = await handler.Handle(
            new StartListeningAttemptCommand(tenantId, Guid.NewGuid(), audio.Id.Value, ListeningMode.Exam), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LISTENING_AUDIO_EMPTY", result.Error.Code);
    }

    [Fact]
    public async Task Cross_tenant_audio_is_not_found_rather_than_forbidden()
    {
        var audios = new FakeListeningAudioRepository();
        var audio = SeedAudio(audios, Guid.NewGuid());
        var handler = new StartListeningAttemptCommandHandler(
            audios, new FakeListeningQuestionRepository(), new FakeListeningAttemptRepository(), new FakeUnitOfWork(), new FakeClock(Now),
            new FakeGuidProvider());

        var result = await handler.Handle(
            new StartListeningAttemptCommand(Guid.NewGuid(), Guid.NewGuid(), audio.Id.Value, ListeningMode.Exam), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LISTENING_AUDIO", result.Error.Code);
    }
}
