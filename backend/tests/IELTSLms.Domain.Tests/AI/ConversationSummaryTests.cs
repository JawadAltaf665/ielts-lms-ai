using IELTSLms.Domain.AI;

namespace IELTSLms.Domain.Tests.AI;

public sealed class ConversationSummaryTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var result = ConversationSummary.Create(
            ConversationSummaryId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()),
            "Learner is working on Task 2 essays.", 10, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.CoversUpToSequence);
    }

    [Fact]
    public void Create_with_blank_summary_text_fails()
    {
        var result = ConversationSummary.Create(
            ConversationSummaryId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()), "   ", 10, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_SUMMARY_TEXT_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Create_with_non_positive_covers_up_to_sequence_fails()
    {
        var result = ConversationSummary.Create(
            ConversationSummaryId.From(Guid.NewGuid()), Guid.NewGuid(), ConversationId.From(Guid.NewGuid()), "summary", 0, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_CONVERSATION_SUMMARY_COVERS_UP_TO_SEQUENCE_INVALID", result.Error.Code);
    }

    [Fact]
    public void There_is_no_way_to_mutate_content_after_creation()
    {
        var contentMutators = typeof(ConversationSummary).GetMethods()
            .Where(m => m.DeclaringType == typeof(ConversationSummary) && m.IsPublic && !m.IsStatic && !m.IsSpecialName)
            .Select(m => m.Name)
            .ToArray();

        Assert.Empty(contentMutators);
    }
}
