using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Tests.Grammar;

public sealed class CreateGrammarTopicCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_topic_and_saves()
    {
        var topics = new FakeGrammarTopicRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateGrammarTopicCommandHandler(topics, unitOfWork, new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateGrammarTopicCommand(tenantId, "Present Perfect", "desc", GrammarTrack.Academic, GrammarDifficulty.Intermediate, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Duplicate_title_in_the_same_scope_is_rejected()
    {
        var topics = new FakeGrammarTopicRepository();
        var tenantId = Guid.NewGuid();
        topics.Seed(GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), tenantId, "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now).Value);

        var handler = new CreateGrammarTopicCommandHandler(topics, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateGrammarTopicCommand(tenantId, "Present Perfect", "desc", GrammarTrack.Academic, GrammarDifficulty.Intermediate, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_GRAMMAR_TOPIC_TITLE_TAKEN", result.Error.Code);
    }

    [Fact]
    public async Task Same_title_in_a_different_tenant_is_allowed()
    {
        var topics = new FakeGrammarTopicRepository();
        topics.Seed(GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), Guid.NewGuid(), "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now).Value);

        var handler = new CreateGrammarTopicCommandHandler(topics, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateGrammarTopicCommand(Guid.NewGuid(), "Present Perfect", "desc", GrammarTrack.Academic, GrammarDifficulty.Intermediate, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
