using IELTSLms.Domain.Grammar;

namespace IELTSLms.Domain.Tests.Grammar;

public sealed class GrammarTopicTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_null_tenant_is_global()
    {
        var result = GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), null, "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_tenant_is_not_global()
    {
        var result = GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), Guid.NewGuid(), "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
    }

    [Fact]
    public void Create_with_blank_title_fails()
    {
        var result = GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), Guid.NewGuid(), "   ", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_GRAMMAR_TITLE_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Update_changes_fields_and_stamps_updated_at()
    {
        var topic = GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), Guid.NewGuid(), "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now).Value;

        var result = topic.Update("Past Perfect", "new desc", GrammarTrack.General, GrammarDifficulty.Advanced, null, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Past Perfect", topic.Title);
        Assert.Equal(GrammarTrack.General, topic.Track);
        Assert.Equal(GrammarDifficulty.Advanced, topic.Difficulty);
        Assert.Equal(Now.AddDays(1), topic.UpdatedAtUtc);
    }

    [Fact]
    public void SoftDelete_is_idempotent()
    {
        var topic = GrammarTopic.Create(
            GrammarTopicId.From(Guid.NewGuid()), Guid.NewGuid(), "Present Perfect", "desc", GrammarTrack.Academic,
            GrammarDifficulty.Intermediate, null, Now).Value;

        topic.SoftDelete(Now);
        var deletedAt = topic.DeletedAtUtc;
        topic.SoftDelete(Now.AddDays(1));

        Assert.True(topic.IsDeleted);
        Assert.Equal(deletedAt, topic.DeletedAtUtc);
    }
}
