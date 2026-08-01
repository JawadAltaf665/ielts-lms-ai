using System.Text;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingAttempt;
using IELTSLms.Application.Tests.Speaking.Fakes;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class CreateSpeakingAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static CreateSpeakingAttemptCommand ValidCommand(Guid tenantId, Guid speakingTaskId, long sizeBytes = 1024) =>
        new(tenantId, Guid.NewGuid(), speakingTaskId, 45, new MemoryStream(Encoding.UTF8.GetBytes("fake-audio-bytes")), "recording.mp3",
            "audio/mpeg", sizeBytes);

    [Fact]
    public async Task Creates_a_recorded_attempt_and_uploads_the_file()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var files = new FakeFileAssetRepository();
        var storage = new FakeFileStorage(new FakeClock(Now));
        var handler = new CreateSpeakingAttemptCommandHandler(
            attempts, files, storage, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(ValidCommand(tenantId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Recorded", result.Value.Status);
        Assert.Single(files.Files);
        Assert.Equal("audio/mpeg", files.Files[0].ContentType);
        Assert.Single(attempts.Attempts);
    }

    [Fact]
    public async Task Rejects_an_oversized_file_before_uploading_anything()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var files = new FakeFileAssetRepository();
        var storage = new FakeFileStorage(new FakeClock(Now));
        var handler = new CreateSpeakingAttemptCommandHandler(
            attempts, files, storage, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            ValidCommand(Guid.NewGuid(), Guid.NewGuid(), sizeBytes: 200 * 1024 * 1024), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_FILE_SIZE_INVALID", result.Error.Code);
        Assert.Empty(files.Files);
        Assert.Empty(attempts.Attempts);
    }
}
