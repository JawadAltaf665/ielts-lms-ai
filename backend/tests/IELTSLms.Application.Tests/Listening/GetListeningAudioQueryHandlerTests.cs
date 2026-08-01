using IELTSLms.Application.Listening.GetListeningAudio;
using IELTSLms.Application.Tests.Listening.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Listening;

/// <summary>
/// Sprint 5 test note: "exam mode never leaks transcript even via a crafted query param." The API
/// endpoint parses the raw query string against a strict enum allowlist (defaulting to Exam), so by
/// the time a <see cref="ListeningMode"/> reaches this handler it can only ever be a real enum member —
/// this asserts the handler's own half of that guarantee: it never returns the transcript for Exam.
/// </summary>
public sealed class GetListeningAudioQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (Guid TenantId, ListeningAudio Audio) SeedAudio(FakeListeningAudioRepository audios)
    {
        var tenantId = Guid.NewGuid();
        var audio = ListeningAudio.Create(
            ListeningAudioId.From(Guid.NewGuid()), tenantId, "Section 1", ReadingTrack.Academic, FileAssetId.From(Guid.NewGuid()), null, 600,
            "The confidential transcript.", Now).Value;
        audios.Seed(audio);
        return (tenantId, audio);
    }

    [Fact]
    public async Task Exam_mode_never_returns_the_transcript()
    {
        var audios = new FakeListeningAudioRepository();
        var (tenantId, audio) = SeedAudio(audios);
        var handler = new GetListeningAudioQueryHandler(audios, new FakeListeningQuestionRepository());

        var result = await handler.Handle(new GetListeningAudioQuery(tenantId, audio.Id.Value, ListeningMode.Exam), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.TranscriptText);
    }

    [Fact]
    public async Task Learning_mode_returns_the_transcript()
    {
        var audios = new FakeListeningAudioRepository();
        var (tenantId, audio) = SeedAudio(audios);
        var handler = new GetListeningAudioQueryHandler(audios, new FakeListeningQuestionRepository());

        var result = await handler.Handle(new GetListeningAudioQuery(tenantId, audio.Id.Value, ListeningMode.Learning), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("The confidential transcript.", result.Value.TranscriptText);
    }
}
