using IELTSLms.Domain.Grammar;

namespace IELTSLms.Domain.Tests.Grammar;

public sealed class GrammarProgressTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static GrammarProgress CreateProgress() => GrammarProgress.Create(
        GrammarProgressId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), GrammarTopicId.From(Guid.NewGuid()), Now);

    [Fact]
    public void Create_starts_not_started_with_zero_attempts()
    {
        var progress = CreateProgress();

        Assert.Equal(GrammarMasteryLevel.NotStarted, progress.MasteryLevel);
        Assert.Equal(0, progress.AttemptCount);
    }

    [Fact]
    public void RecordAttempt_increments_count_and_stamps_last_attempted()
    {
        var progress = CreateProgress();

        progress.RecordAttempt(rawScore: 4, maxScore: 5, Now);

        Assert.Equal(1, progress.AttemptCount);
        Assert.Equal(Now, progress.LastAttemptedAtUtc);
    }

    [Theory]
    [InlineData(1, 1, 5, GrammarMasteryLevel.Learning)] // low accuracy
    [InlineData(1, 4, 5, GrammarMasteryLevel.Practiced)] // high accuracy, first attempt: not yet mastered
    [InlineData(3, 5, 5, GrammarMasteryLevel.Mastered)] // high accuracy sustained over 3 attempts
    [InlineData(1, 0, 5, GrammarMasteryLevel.Learning)] // zero score
    public void RecordAttempt_computes_mastery_from_accuracy_and_repetition(
        int attemptsToApply, int rawScore, int maxScore, GrammarMasteryLevel expected)
    {
        var progress = CreateProgress();

        for (var i = 0; i < attemptsToApply; i++)
        {
            progress.RecordAttempt(rawScore, maxScore, Now);
        }

        Assert.Equal(expected, progress.MasteryLevel);
    }

    [Fact]
    public void RecordAttempt_with_zero_max_score_does_not_throw_and_is_not_mastered()
    {
        var progress = CreateProgress();

        progress.RecordAttempt(rawScore: 0, maxScore: 0, Now);

        Assert.Equal(GrammarMasteryLevel.Learning, progress.MasteryLevel);
    }
}
