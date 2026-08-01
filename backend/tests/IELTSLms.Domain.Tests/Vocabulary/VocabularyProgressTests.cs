using IELTSLms.Domain.Vocabulary;
using IELTSLms.Domain.Vocabulary.Events;

namespace IELTSLms.Domain.Tests.Vocabulary;

public sealed class VocabularyProgressTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static VocabularyProgress CreateProgress() => VocabularyProgress.Create(
        VocabularyProgressId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), VocabularyWordId.From(Guid.NewGuid()), Now);

    [Fact]
    public void Create_seeds_the_standard_SM2_starting_state()
    {
        var progress = CreateProgress();

        Assert.Equal(2.5m, progress.EaseFactor);
        Assert.Equal(0, progress.RepetitionCount);
        Assert.Equal(0, progress.IntervalDays);
        Assert.Null(progress.NextReviewAtUtc);
        Assert.Null(progress.LastGrade);
    }

    [Fact]
    public void Grade_advances_the_schedule_and_stamps_next_review_and_last_grade()
    {
        var progress = CreateProgress();

        progress.Grade(quality: 4, Now);

        Assert.Equal(1, progress.RepetitionCount);
        Assert.Equal(1, progress.IntervalDays);
        Assert.Equal(4, progress.LastGrade);
        Assert.Equal(Now.AddDays(1), progress.NextReviewAtUtc);
    }

    [Fact]
    public void Grade_raises_VocabularyCompleted_with_the_graded_quality()
    {
        var progress = CreateProgress();

        progress.Grade(quality: 5, Now);

        var raised = Assert.Single(progress.DomainEvents);
        var completed = Assert.IsType<VocabularyCompletedDomainEvent>(raised);
        Assert.Equal(progress.UserId, completed.UserId);
        Assert.Equal(progress.VocabularyWordId.Value, completed.VocabularyWordId);
        Assert.Equal(5, completed.Grade);
    }

    [Fact]
    public void A_failed_review_resets_the_schedule_but_still_advances_next_review_by_one_day()
    {
        var progress = CreateProgress();
        progress.Grade(quality: 5, Now);
        progress.Grade(quality: 4, Now.AddDays(1));

        progress.Grade(quality: 1, Now.AddDays(7));

        Assert.Equal(0, progress.RepetitionCount);
        Assert.Equal(1, progress.IntervalDays);
        Assert.Equal(Now.AddDays(8), progress.NextReviewAtUtc);
    }
}
