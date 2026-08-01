namespace IELTSLms.Domain.Vocabulary;

/// <summary>
/// Simplified SM-2 spaced-repetition scheduling (Sprint-2-Vocabulary-Module.md "SM-2 scheduling
/// algorithm"). Pure and framework-free so it is exhaustively unit-testable without mocks — the
/// sprint's highest-risk pure logic, same weight as Sprint 1's <see cref="Grammar.GrammarAnswerGrader"/>.
/// </summary>
public static class SpacedRepetitionScheduler
{
    /// <summary>The floor SM-2 imposes on ease factor so a run of failures never makes intervals grow negative or zero.</summary>
    public const decimal MinimumEaseFactor = 1.3m;

    /// <summary>
    /// Folds one review of quality <paramref name="quality"/> (0-5) into the current schedule state,
    /// returning the next ease factor, interval, and repetition count.
    /// </summary>
    public static SpacedRepetitionResult Schedule(decimal easeFactor, int intervalDays, int repetitionCount, int quality)
    {
        // Interval uses the ease factor as it stood coming into this review; EF is only adjusted for
        // the *next* review afterward (canonical SM-2 order — see Sprint-2-Vocabulary-Module.md).
        var newEaseFactor = AdjustEaseFactor(easeFactor, quality);

        if (quality < 3)
        {
            return new SpacedRepetitionResult(newEaseFactor, IntervalDays: 1, RepetitionCount: 0);
        }

        var newRepetitionCount = repetitionCount + 1;
        var newIntervalDays = newRepetitionCount switch
        {
            1 => 1,
            2 => 6,
            _ => (int)Math.Round(intervalDays * easeFactor, MidpointRounding.AwayFromZero),
        };

        return new SpacedRepetitionResult(newEaseFactor, newIntervalDays, newRepetitionCount);
    }

    private static decimal AdjustEaseFactor(decimal easeFactor, int quality)
    {
        var qualityGap = 5 - quality;
        var delta = 0.1m - qualityGap * (0.08m + qualityGap * 0.02m);
        return Math.Max(MinimumEaseFactor, easeFactor + delta);
    }
}

/// <summary>The updated schedule state produced by <see cref="SpacedRepetitionScheduler.Schedule"/>.</summary>
public readonly record struct SpacedRepetitionResult(decimal EaseFactor, int IntervalDays, int RepetitionCount);
