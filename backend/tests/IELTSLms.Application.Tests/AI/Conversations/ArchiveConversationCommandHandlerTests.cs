using IELTSLms.Application.AI.Conversations.Commands.ArchiveConversation;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Conversations;

public sealed class ArchiveConversationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Archives_the_callers_own_conversation()
    {
        var conversations = new FakeConversationRepository();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        conversations.Seed(conversation);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ArchiveConversationCommandHandler(conversations, unitOfWork, new FakeClock(Now.AddDays(1)));

        var result = await handler.Handle(new ArchiveConversationCommand(_tenantId, _userId, conversation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ConversationStatus.Archived, conversation.Status);
        Assert.True(conversation.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Archiving_another_users_conversation_is_not_found()
    {
        var conversations = new FakeConversationRepository();
        var foreign = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, Guid.NewGuid(), "ai-tutor", Now).Value;
        conversations.Seed(foreign);
        var handler = new ArchiveConversationCommandHandler(conversations, new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(new ArchiveConversationCommand(_tenantId, _userId, foreign.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_CONVERSATION", result.Error.Code);
        Assert.False(foreign.IsDeleted);
    }
}
