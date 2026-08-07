using IELTSLms.Application.AI.Conversations.Queries.ListConversations;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Conversations;

public sealed class ListConversationsQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Lists_only_the_callers_own_conversations_newest_first()
    {
        var conversations = new FakeConversationRepository();
        var mine1 = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        var mine2 = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now.AddMinutes(5)).Value;
        var someoneElse = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, Guid.NewGuid(), "ai-tutor", Now).Value;
        conversations.Seed(mine1);
        conversations.Seed(mine2);
        conversations.Seed(someoneElse);
        var handler = new ListConversationsQueryHandler(conversations);

        var result = await handler.Handle(
            new ListConversationsQuery(_tenantId, _userId, null, null, null, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(mine2.Id.Value, result.Value.Items[0].Id);
        Assert.Equal(mine1.Id.Value, result.Value.Items[1].Id);
    }

    [Fact]
    public async Task Filters_by_status()
    {
        var conversations = new FakeConversationRepository();
        var active = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        var archived = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        archived.Archive(Now);
        conversations.Seed(active);
        conversations.Seed(archived);
        var handler = new ListConversationsQueryHandler(conversations);

        var result = await handler.Handle(
            new ListConversationsQuery(_tenantId, _userId, null, "Archived", null, 20), CancellationToken.None);

        var item = Assert.Single(result.Value.Items);
        Assert.Equal(archived.Id.Value, item.Id);
    }

    [Fact]
    public async Task Cursor_slices_to_the_next_page()
    {
        var conversations = new FakeConversationRepository();
        var older = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now).Value;
        var newer = Conversation.Create(ConversationId.From(Guid.NewGuid()), _tenantId, _userId, "ai-tutor", Now.AddMinutes(5)).Value;
        conversations.Seed(older);
        conversations.Seed(newer);
        var handler = new ListConversationsQueryHandler(conversations);

        var firstPage = await handler.Handle(
            new ListConversationsQuery(_tenantId, _userId, null, null, null, 1), CancellationToken.None);
        Assert.NotNull(firstPage.Value.NextCursor);

        var secondPage = await handler.Handle(
            new ListConversationsQuery(_tenantId, _userId, null, null, firstPage.Value.NextCursor, 1), CancellationToken.None);

        var item = Assert.Single(secondPage.Value.Items);
        Assert.Equal(older.Id.Value, item.Id);
        Assert.Null(secondPage.Value.NextCursor);
    }
}
