using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class ConversationMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var result = ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()),
            ConversationMessageRole.User, "Hello", 1, 5, null, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ConversationMessageRole.User, result.Value.Role);
        Assert.Equal(1, result.Value.Sequence);
    }

    [Fact]
    public void Create_with_blank_content_fails()
    {
        var result = ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()),
            ConversationMessageRole.User, "   ", 1, 5, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_MESSAGE_CONTENT_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_non_positive_sequence_fails()
    {
        var result = ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()),
            ConversationMessageRole.User, "Hello", 0, 5, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_MESSAGE_SEQUENCE_INVALID", result.Error.Code);
    }

    [Fact]
    public void Create_with_negative_token_count_fails()
    {
        var result = ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()),
            ConversationMessageRole.User, "Hello", 1, -1, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_MESSAGE_TOKEN_COUNT_INVALID", result.Error.Code);
    }

    [Fact]
    public void There_is_no_way_to_mutate_content_after_creation()
    {
        // Append-only enforcement (Phase 7 Sprint 3): a correction is always a new message, never an
        // edit — the domain type exposes no public instance method surface at all beyond what Entity
        // itself already provides (ClearDomainEvents), so no Application-layer code could even attempt one.
        var contentMutators = typeof(ConversationMessage).GetMethods()
            .Where(m => m.DeclaringType == typeof(ConversationMessage) && m.IsPublic && !m.IsStatic && !m.IsSpecialName)
            .Select(m => m.Name)
            .ToArray();

        Assert.Empty(contentMutators);
    }
}
