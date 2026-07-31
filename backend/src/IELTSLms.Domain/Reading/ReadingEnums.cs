namespace IELTSLms.Domain.Reading;

/// <summary>IELTS track a <see cref="ReadingPassage"/> targets (ERD ENT-021).</summary>
public enum ReadingTrack
{
    Academic = 1,
    General = 2,
    Both = 3,
}

/// <summary>Difficulty tier of a <see cref="ReadingPassage"/> (ERD ENT-021).</summary>
public enum ReadingDifficulty
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
}

/// <summary>
/// CB-IELTS-style question type for a <see cref="ReadingQuestion"/> (ERD ENT-022 says only "type ∈
/// supported set" — this is the sprint's explicit minimum viable set). Each maps to a distinct
/// <see cref="ReadingAnswerGrader"/> normalization strategy.
/// </summary>
public enum ReadingQuestionType
{
    MultipleChoice = 1,
    TrueFalseNotGiven = 2,
    YesNoNotGiven = 3,
    MatchingHeadings = 4,
    MatchingInformation = 5,
    SentenceCompletion = 6,
    SummaryCompletion = 7,
    ShortAnswer = 8,
}
