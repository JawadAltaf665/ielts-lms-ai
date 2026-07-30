using IELTSLms.Application.Vocabulary.GradeVocabularyReview;
using IELTSLms.Application.Tests.Vocabulary.Fakes;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.Domain.Vocabulary.Events;

namespace IELTSLms.Application.Tests.Vocabulary;

public sealed class GradeVocabularyReviewCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static VocabularyWord SeedWord(FakeVocabularyWordRepository words, Guid tenantId)
    {
        var word = VocabularyWord.Create(
            VocabularyWordId.From(Guid.NewGuid()), tenantId, "bank", "financial institution", "meaning", "[]",
            VocabularyFrequencyBand.High, null, null, VocabularyDifficultyLevel.Intermediate, Now).Value;
        words.Seed(word);
        return word;
    }

    [Fact]
    public async Task First_grade_seeds_progress_at_the_standard_SM2_starting_state_and_advances_it()
    {
        var words = new FakeVocabularyWordRepository();
        var progress = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var word = SeedWord(words, tenantId);
        var userId = Guid.NewGuid();

        var handler = new GradeVocabularyReviewCommandHandler(
            words, progress, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(new GradeVocabularyReviewCommand(tenantId, userId, word.Id.Value, 4), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.RepetitionCount);
        Assert.Equal(1, result.Value.IntervalDays);
        Assert.Equal(4, result.Value.LastGrade);
        Assert.Single(progress.Progress);
    }

    [Fact]
    public async Task Second_grade_updates_the_same_progress_row_not_a_new_one()
    {
        var words = new FakeVocabularyWordRepository();
        var progress = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var word = SeedWord(words, tenantId);
        var userId = Guid.NewGuid();
        var handler = new GradeVocabularyReviewCommandHandler(
            words, progress, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        await handler.Handle(new GradeVocabularyReviewCommand(tenantId, userId, word.Id.Value, 4), CancellationToken.None);
        var second = await handler.Handle(new GradeVocabularyReviewCommand(tenantId, userId, word.Id.Value, 4), CancellationToken.None);

        Assert.Single(progress.Progress);
        Assert.Equal(2, second.Value.RepetitionCount);
        Assert.Equal(6, second.Value.IntervalDays);
    }

    [Fact]
    public async Task Grading_raises_VocabularyCompleted()
    {
        var words = new FakeVocabularyWordRepository();
        var progress = new FakeVocabularyProgressRepository();
        var tenantId = Guid.NewGuid();
        var word = SeedWord(words, tenantId);
        var userId = Guid.NewGuid();
        var handler = new GradeVocabularyReviewCommandHandler(
            words, progress, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        await handler.Handle(new GradeVocabularyReviewCommand(tenantId, userId, word.Id.Value, 5), CancellationToken.None);

        var row = Assert.Single(progress.Progress);
        var raised = Assert.Single(row.DomainEvents);
        var completed = Assert.IsType<VocabularyCompletedDomainEvent>(raised);
        Assert.Equal(word.Id.Value, completed.VocabularyWordId);
        Assert.Equal(5, completed.Grade);
    }

    [Fact]
    public async Task Missing_word_is_rejected()
    {
        var handler = new GradeVocabularyReviewCommandHandler(
            new FakeVocabularyWordRepository(), new FakeVocabularyProgressRepository(), new FakeUnitOfWork(), new FakeClock(Now),
            new FakeGuidProvider());

        var result = await handler.Handle(
            new GradeVocabularyReviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_VOCABULARY_WORD", result.Error.Code);
    }
}
