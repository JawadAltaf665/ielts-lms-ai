namespace IELTSLms.Domain.Assessment;

/// <summary>Lifecycle state of a <see cref="MonthlyMockExam"/> session (ERD ENT-036).</summary>
public enum MockExamState
{
    InProgress,
    Completed,
    Abandoned,
}

/// <summary>The four skills composed into one mock exam session (DDD §13.5 workflow).</summary>
public enum MockExamSectionType
{
    Reading,
    Listening,
    Writing,
    Speaking,
}

/// <summary>
/// Server-owned section durations for the two sections whose own skill module has no timed "start"
/// phase to reuse: <c>WritingTask</c>/<c>SpeakingTask</c> carry no <c>DurationSeconds</c> (unlike
/// <c>ReadingPassage</c>/<c>ListeningAudio</c>, whose own configured duration is reused as-is via their
/// existing <c>Start&lt;Skill&gt;Attempt</c> command). Values mirror the real IELTS test structure:
/// Writing 60 minutes; Speaking ~11-14 minutes, rounded up to a 15-minute server allowance.
/// </summary>
public static class MockExamSectionDurations
{
    public const int WritingSeconds = 3600;
    public const int SpeakingSeconds = 900;
}
