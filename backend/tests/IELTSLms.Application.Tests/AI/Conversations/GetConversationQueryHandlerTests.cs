using IELTSLms.Application.AI.Conversations.Queries.GetConversation;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Conversations;

public sealed class GetConversationQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Returns_the_conversation_with_a_page_of_messages_newest_first()
    {
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        conversations.Seed(conversation);
        for (var sequence = 1; sequence <= 3; sequence++)
        {
            messages.Seed(ConversationMessage.Create(
                ConversationMessageId.From(Guid.NewGuid()), _tenantId, conversation.Id, ConversationMessageRole.User,
                $"turn {sequence}", sequence, 5, null, Now).Value);
        }

        var handler = new GetConversationQueryHandler(conversations, messages);
        var result = await handler.Handle(
            new GetConversationQuery(_tenantId, _userId, conversation.Id.Value, null, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(conversation.Id.Value, result.Value.Conversation.Id);
        Assert.Equal(3, result.Value.Messages.Items.Count);
        Assert.Equal(3, result.Value.Messages.Items[0].Sequence);
        Assert.Null(result.Value.Messages.NextCursor);
    }

    [Fact]
    public async Task Conversation_owned_by_another_user_is_not_found()
    {
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var foreign = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, Guid.NewGuid(), "ai-tutor", Now).Value;
        conversations.Seed(foreign);
        var handler = new GetConversationQueryHandler(conversations, messages);

        var result = await handler.Handle(
            new GetConversationQuery(_tenantId, _userId, foreign.Id.Value, null, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_CONVERSATION", result.Error.Code);
    }

    [Fact]
    public async Task Conversation_in_another_tenant_is_not_found()
    {
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var foreign = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), _userId, "ai-tutor", Now).Value;
        conversations.Seed(foreign);
        var handler = new GetConversationQueryHandler(conversations, messages);

        var result = await handler.Handle(
            new GetConversationQuery(_tenantId, _userId, foreign.Id.Value, null, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_CONVERSATION", result.Error.Code);
    }

    [Fact]
    public async Task Missing_conversation_is_not_found()
    {
        var handler = new GetConversationQueryHandler(new FakeConversationRepository(), new FakeConversationMessageRepository());

        var result = await handler.Handle(
            new GetConversationQuery(_tenantId, _userId, Guid.NewGuid(), null, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_CONVERSATION", result.Error.Code);
    }
}
