using IELTSLms.Application.Listening.RegisterListeningReplay;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Listening;

public sealed class RegisterListeningReplayCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, Guid UserId, ListeningAttempt Attempt) SeedAttempt(
        FakeListeningAudioRepository audios, FakeFileAssetRepository files, ListeningMode mode, int durationSeconds = 1800)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileId = FileAssetId.From(Guid.NewGuid());
        var file = FileAsset.Create(
            fileId, tenantId, userId, "listening-audio/x.mp3", "x.mp3", "audio/mpeg", 1024, ["audio/mpeg"], 2048, Now).Value;
        files.Seed(file);

        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Section 1", ReadingTrack.Academic, fileId, null, durationSeconds, null, Now).Value;
        audios.Seed(audio);

        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, audio.Id, mode, Now, durationSeconds);

        return (tenantId, userId, attempt);
    }

    private static RegisterListeningReplayCommandHandler BuildHandler(
        FakeListeningAttemptRepository attempts, FakeListeningAudioRepository audios, FakeFileAssetRepository files, DateTimeOffset now) =>
        new(attempts, audios, files, new FakeFileStorage(new FakeClock(now)), new FakeTenantContextAccessor(), new FakeUnitOfWork(), new FakeClock(now));

    [Fact]
    public async Task Exam_mode_rejects_the_first_replay_with_409_business_replay_limit_exceeded()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(audios, files, ListeningMode.Exam);
        attempts.Seed(attempt);
        var handler = BuildHandler(attempts, audios, files, Now.AddMinutes(1));

        var result = await handler.Handle(new RegisterListeningReplayCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_REPLAY_LIMIT_EXCEEDED", result.Error.Code);
    }

    [Fact]
    public async Task Learning_mode_issues_a_fresh_signed_url_and_increments_the_count()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(audios, files, ListeningMode.Learning);
        attempts.Seed(attempt);
        var handler = BuildHandler(attempts, audios, files, Now.AddMinutes(1));

        var result = await handler.Handle(new RegisterListeningReplayCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ReplayCount);
        Assert.Contains("listening-audio/x.mp3", result.Value.Url);
        Assert.Equal(1, attempt.ReplayCount);
    }

    [Fact]
    public async Task Registering_a_replay_after_the_deadline_is_rejected()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var attempts = new FakeListeningAttemptRepository();
        var (tenantId, userId, attempt) = SeedAttempt(audios, files, ListeningMode.Learning, durationSeconds: 60);
        attempts.Seed(attempt);
        var handler = BuildHandler(attempts, audios, files, Now.AddSeconds(61));

        var result = await handler.Handle(new RegisterListeningReplayCommand(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LISTENING_ATTEMPT_EXPIRED", result.Error.Code);
        Assert.Equal(0, attempt.ReplayCount);
    }

    [Fact]
    public async Task Missing_attempt_is_rejected()
    {
        var handler = BuildHandler(
            new FakeListeningAttemptRepository(), new FakeListeningAudioRepository(), new FakeFileAssetRepository(), Now);

        var result = await handler.Handle(new RegisterListeningReplayCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LISTENING_ATTEMPT", result.Error.Code);
    }
}
