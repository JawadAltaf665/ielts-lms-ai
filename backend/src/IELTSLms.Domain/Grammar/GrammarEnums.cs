namespace IELTSLms.Domain.Grammar;

/// <summary>IELTS track a <see cref="GrammarTopic"/> targets (ERD ENT-016).</summary>
public enum GrammarTrack
{
    Academic = 1,
    General = 2,
    Both = 3,
}

/// <summary>Difficulty tier of a <see cref="GrammarTopic"/> (ERD ENT-016).</summary>
public enum GrammarDifficulty
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
}

/// <summary>Whether a <see cref="GrammarExercise"/> is low-stakes practice or a scored quiz.</summary>
public enum GrammarExerciseMode
{
    Practice = 1,
    Quiz = 2,
}

/// <summary>Answer-matching shape for a <see cref="GrammarQuestion"/>, mirrored on ENT-022 ReadingQuestions.</summary>
public enum GrammarQuestionType
{
    MultipleChoice = 1,
    FillBlank = 2,
    ErrorCorrection = 3,
    Reorder = 4,
}

/// <summary>Learner mastery tier for a <see cref="GrammarProgress"/> row.</summary>
public enum GrammarMasteryLevel
{
    NotStarted = 1,
    Learning = 2,
    Practiced = 3,
    Mastered = 4,
}
