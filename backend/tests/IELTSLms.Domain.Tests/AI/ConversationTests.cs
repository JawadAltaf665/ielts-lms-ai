using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class ConversationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_starts_Active()
    {
        var result = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "ai-tutor", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ConversationStatus.Active, result.Value.Status);
        Assert.Equal("ai-tutor", result.Value.Feature);
        Assert.Equal(Now, result.Value.StartedAtUtc);
        Assert.Equal(Now, result.Value.LastMessageAtUtc);
    }

    [Fact]
    public void Create_with_blank_feature_fails()
    {
        var result = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "  ", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_FEATURE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Archive_sets_Archived_status_and_soft_deletes()
    {
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "ai-tutor", Now).Value;

        conversation.Archive(Now.AddDays(1));

        Assert.Equal(ConversationStatus.Archived, conversation.Status);
        Assert.True(conversation.IsDeleted);
        Assert.Equal(Now.AddDays(1), conversation.DeletedAtUtc);
        Assert.Equal(Now.AddDays(1), conversation.UpdatedAtUtc);
    }

    [Fact]
    public void RecordMessage_bumps_LastMessageAtUtc()
    {
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "ai-tutor", Now).Value;

        conversation.RecordMessage(Now.AddMinutes(5));

        Assert.Equal(Now.AddMinutes(5), conversation.LastMessageAtUtc);
        Assert.Equal(Now.AddMinutes(5), conversation.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_is_idempotent()
    {
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), "ai-tutor", Now).Value;
        conversation.Archive(Now.AddDays(1));

        conversation.Archive(Now.AddDays(2));

        Assert.Equal(Now.AddDays(1), conversation.DeletedAtUtc);
        Assert.Equal(Now.AddDays(1), conversation.UpdatedAtUtc);
    }
}
