using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Application.Tests.Reading.Fakes;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Tests.Reading;

public sealed class CreateReadingPassageCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_passage()
    {
        var passages = new FakeReadingPassageRepository();
        var handler = new CreateReadingPassageCommandHandler(passages, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateReadingPassageCommand(tenantId, "The Ocean", "Long text...", ReadingTrack.Academic, ReadingDifficulty.Intermediate, null, 3600),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.False(result.Value.IsGlobal);
        Assert.Equal(3600, result.Value.DurationSeconds);
    }

    [Fact]
    public async Task Rejects_a_duplicate_title_within_the_same_tenant()
    {
        var passages = new FakeReadingPassageRepository();
        var tenantId = Guid.NewGuid();
        passages.Seed(ReadingPassage.Create(
            ReadingPassageId.From(Guid.NewGuid()), tenantId, "The Ocean", "Text", ReadingTrack.Academic, ReadingDifficulty.Beginner, null, 3600, Now).Value);
        var handler = new CreateReadingPassageCommandHandler(passages, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateReadingPassageCommand(tenantId, "The Ocean", "Other text", ReadingTrack.Academic, ReadingDifficulty.Beginner, null, 1800),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_READING_PASSAGE_TITLE_TAKEN", result.Error.Code);
    }
}
