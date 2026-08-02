using IELTSLms.Application.AI.Tutor.Commands.StartTutorConversation;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;

namespace IELTSLms.Application.Tests.AI.Tutor;

public sealed class StartTutorConversationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Not_entitled_is_rejected_without_creating_a_conversation()
    {
        var conversations = new FakeConversationRepository();
        var handler = new StartTutorConversationCommandHandler(
            conversations, new FakeUnitOfWork(), new FakeEntitlementService { HasFeature = false }, new FakeClock(Now),
            new FakeGuidProvider());

        var result = await handler.Handle(new StartTutorConversationCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
    }

    [Fact]
    public async Task Entitled_creates_an_active_ai_tutor_conversation()
    {
        var conversations = new FakeConversationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new StartTutorConversationCommandHandler(
            conversations, unitOfWork, new FakeEntitlementService(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new StartTutorConversationCommand(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ai-tutor", result.Value.Feature);
        Assert.Equal("Active", result.Value.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }
}
