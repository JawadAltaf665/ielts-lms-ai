using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Tests.Vocabulary;

public sealed class CreateVocabularyWordCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_word_and_saves()
    {
        var words = new FakeVocabularyWordRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateVocabularyWordCommandHandler(words, unitOfWork, new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateVocabularyWordCommand(
                tenantId, "bank", "financial institution", "a place that holds money", ["the bank approved the loan"],
                VocabularyFrequencyBand.High, "finance", null, VocabularyDifficultyLevel.Intermediate),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Single(result.Value.Examples);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Duplicate_word_and_sense_in_the_same_scope_is_rejected()
    {
        var words = new FakeVocabularyWordRepository();
        var tenantId = Guid.NewGuid();
        words.Seed(VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), tenantId, "bank", "financial institution", "meaning", "[]",
            VocabularyFrequencyBand.High, null, null, VocabularyDifficultyLevel.Intermediate, Now).Value);

        var handler = new CreateVocabularyWordCommandHandler(words, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateVocabularyWordCommand(
                tenantId, "bank", "financial institution", "another meaning", [], VocabularyFrequencyBand.High, null, null,
                VocabularyDifficultyLevel.Intermediate),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_VOCABULARY_WORD_TAKEN", result.Error.Code);
    }

    [Fact]
    public async Task Same_word_with_a_different_sense_is_allowed()
    {
        var words = new FakeVocabularyWordRepository();
        var tenantId = Guid.NewGuid();
        words.Seed(VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), tenantId, "bank", "financial institution", "meaning", "[]",
            VocabularyFrequencyBand.High, null, null, VocabularyDifficultyLevel.Intermediate, Now).Value);

        var handler = new CreateVocabularyWordCommandHandler(words, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateVocabularyWordCommand(
                tenantId, "bank", "riverbank", "the land alongside a river", [], VocabularyFrequencyBand.Low, null, null,
                VocabularyDifficultyLevel.Advanced),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
