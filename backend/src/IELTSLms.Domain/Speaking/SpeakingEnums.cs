namespace IELTSLms.Domain.Speaking;

/// <summary>IELTS Speaking task part (ERD ENT-030).</summary>
public enum SpeakingPart
{
    Part1 = 1,
    Part2 = 2,
    Part3 = 3,
}

/// <summary>Lifecycle of a <see cref="SpeakingAttempt"/> (ERD ENT-031). Starts one state further along
/// than <c>WritingSubmissionStatus.Draft</c> — the recording is uploaded synchronously at creation, so
/// there is no draft-without-audio state to model.</summary>
public enum SpeakingAttemptStatus
{
    Recorded = 1,
    Submitted = 2,
    Evaluated = 3,
    Failed = 4,
}
