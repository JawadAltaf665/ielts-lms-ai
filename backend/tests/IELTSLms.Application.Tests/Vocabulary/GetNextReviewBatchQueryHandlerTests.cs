using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Application.Vocabulary;
using IELTSLms.Application.Vocabulary.GetNextReviewBatch;
using IELTSLms.Domain.Vocabulary;
using Microsoft.Extensions.Options;

namespace IELTSLms.Application.Tests.Vocabulary;

public sealed class GetNextReviewBatchQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static VocabularyWord SeedWord(FakeVocabularyWordRepository words, Guid tenantId, string word) =>
        Seed(words, VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), tenantId, word, "sense", "meaning", "[]", VocabularyFrequencyBand.High, null, null,
            VocabularyDifficultyLevel.Intermediate, Now).Value);

    private static VocabularyWord Seed(FakeVocabularyWordRepository words, VocabularyWord word)
    {
        words.Seed(word);
        return word;
    }

    private static GetNextReviewBatchQueryHandler CreateHandler(
        FakeVocabularyWordRepository words, FakeVocabularyProgressRepository progress, int defaultBatchSize = 20, int maxBatchSize = 50) =>
        new(words, progress, Options.Create(new VocabularyOptions { DefaultReviewBatchSize = defaultBatchSize, MaxReviewBatchSize = maxBatchSize }),
            new FakeClock(Now));

    [Fact]
    public async Task Due_words_are_returned_before_new_words()
    {
        var words = new FakeVocabularyWordRepository();
        var progressRepo = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dueWord = SeedWord(words, tenantId, "due-word");
        var dueProgress = VocabularyProgress.Create(VocabularyProgressId.From(Guid.NewGuid()), tenantId, userId, dueWord.Id, Now);
        dueProgress.Grade(4, Now.AddDays(-2)); // NextReviewAtUtc = Now - 1 day, already due
        progressRepo.Seed(dueProgress);
        words.MarkHasProgress(dueWord.Id);

        SeedWord(words, tenantId, "new-word");

        var handler = CreateHandler(words, progressRepo, defaultBatchSize: 1);

        var result = await handler.Handle(new GetNextReviewBatchQuery(tenantId, userId, BatchSize: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(dueWord.Id.Value, item.Word.Id);
        Assert.NotNull(item.Progress);
    }

    [Fact]
    public async Task Batch_is_filled_out_with_new_words_when_fewer_than_the_batch_size_are_due()
    {
        var words = new FakeVocabularyWordRepository();
        var progressRepo = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SeedWord(words, tenantId, "new-word-1");
        SeedWord(words, tenantId, "new-word-2");

        var handler = CreateHandler(words, progressRepo, defaultBatchSize: 5);

        var result = await handler.Handle(new GetNextReviewBatchQuery(tenantId, userId, BatchSize: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, item => Assert.Null(item.Progress));
    }

    [Fact]
    public async Task Requested_batch_size_is_clamped_to_the_configured_maximum()
    {
        var words = new FakeVocabularyWordRepository();
        var progressRepo = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        for (var i = 0; i < 10; i++)
        {
            SeedWord(words, tenantId, $"word-{i}");
        }

        var handler = CreateHandler(words, progressRepo, defaultBatchSize: 20, maxBatchSize: 3);

        var result = await handler.Handle(new GetNextReviewBatchQuery(tenantId, userId, BatchSize: 100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
    }
}
