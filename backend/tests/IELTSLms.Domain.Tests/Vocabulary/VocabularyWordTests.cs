using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Domain.Tests.Vocabulary;

public sealed class VocabularyWordTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static VocabularyWord CreateWord(Guid? tenantId = null) => VocabularyWord.Create(
        VocabularyWordId.From(Guid.NewGuid()), tenantId ?? Guid.NewGuid(), "bank", "financial institution", "a place that holds money",
        "[]", VocabularyFrequencyBand.High, "finance", null, VocabularyDifficultyLevel.Intermediate, Now).Value;

    [Fact]
    public void Create_with_tenant_id_is_not_global()
    {
        var word = CreateWord();

        Assert.False(word.IsGlobal);
    }

    [Fact]
    public void Create_with_null_tenant_id_is_global()
    {
        var word = VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), null, "bank", "financial institution", "a place that holds money", "[]",
            VocabularyFrequencyBand.High, "finance", null, VocabularyDifficultyLevel.Intermediate, Now);

        Assert.True(word.Value.IsGlobal);
    }

    [Theory]
    [InlineData("", "sense", "meaning")]
    [InlineData("word", "", "meaning")]
    [InlineData("word", "sense", "")]
    public void Create_rejects_blank_required_fields(string word, string sense, string meaning)
    {
        var result = VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), Guid.NewGuid(), word, sense, meaning, "[]", VocabularyFrequencyBand.High, null, null,
            VocabularyDifficultyLevel.Beginner, Now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Update_changes_content_but_not_the_headword()
    {
        var word = CreateWord();

        var result = word.Update(
            "riverbank", "the land alongside a river", "[\"the boat reached the bank\"]", VocabularyFrequencyBand.Low, "geography",
            null, VocabularyDifficultyLevel.Advanced, Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal("bank", word.Word);
        Assert.Equal("riverbank", word.Sense);
        Assert.Equal(VocabularyDifficultyLevel.Advanced, word.DifficultyLevel);
    }

    [Fact]
    public void SoftDelete_is_idempotent()
    {
        var word = CreateWord();

        word.SoftDelete(Now);
        word.SoftDelete(Now.AddDays(1));

        Assert.True(word.IsDeleted);
        Assert.Equal(Now, word.DeletedAtUtc);
    }
}
