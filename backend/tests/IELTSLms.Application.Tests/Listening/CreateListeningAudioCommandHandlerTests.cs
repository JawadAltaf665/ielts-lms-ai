using System.Text;
using IELTSLms.Application.Listening.CreateListeningAudio;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Listening;

public sealed class CreateListeningAudioCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static CreateListeningAudioCommand ValidCommand(Guid tenantId, string title = "Section 1", long sizeBytes = 1024) =>
        new(tenantId, Guid.NewGuid(), title, ReadingTrack.Academic, 600, null, "A transcript.",
            new MemoryStream(Encoding.UTF8.GetBytes("fake-audio-bytes")), "track.mp3", "audio/mpeg", sizeBytes);

    [Fact]
    public async Task Creates_a_tenant_owned_audio_and_uploads_the_file()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var storage = new FakeFileStorage(new FakeClock(Now));
        var handler = new CreateListeningAudioCommandHandler(audios, files, storage, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(ValidCommand(tenantId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.False(result.Value.IsGlobal);
        Assert.Single(files.Files);
        Assert.Equal("audio/mpeg", files.Files[0].ContentType);
    }

    [Fact]
    public async Task Rejects_a_duplicate_title_within_the_same_tenant_without_touching_storage()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var storage = new FakeFileStorage(new FakeClock(Now));
        var tenantId = Guid.NewGuid();
        audios.Seed(ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Section 1", ReadingTrack.Academic,
            FileAssetId.From(Guid.NewGuid()), null, 600, null, Now).Value);
        var handler = new CreateListeningAudioCommandHandler(audios, files, storage, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(ValidCommand(tenantId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LISTENING_AUDIO_TITLE_TAKEN", result.Error.Code);
        Assert.Empty(files.Files);
    }

    [Fact]
    public async Task Rejects_an_oversized_file_before_uploading_anything()
    {
        var audios = new FakeListeningAudioRepository();
        var files = new FakeFileAssetRepository();
        var storage = new FakeFileStorage(new FakeClock(Now));
        var handler = new CreateListeningAudioCommandHandler(audios, files, storage, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(ValidCommand(Guid.NewGuid(), sizeBytes: 200 * 1024 * 1024), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_FILE_SIZE_INVALID", result.Error.Code);
        Assert.Empty(files.Files);
    }
}
