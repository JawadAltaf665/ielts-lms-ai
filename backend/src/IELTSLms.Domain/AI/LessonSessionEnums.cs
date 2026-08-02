namespace IELTSLms.Domain.AI;

/// <summary>The AI Teacher's fixed step sequence for a <see cref="LessonSession"/> (Phase 7 Sprint 4):
/// explain → check understanding → practice → recap → completed. Linear and non-skippable — the only
/// branch is <see cref="CheckUnderstanding"/> repeating itself on a failed check.</summary>
public enum LessonStep
{
    Explain = 1,
    CheckUnderstanding = 2,
    Practice = 3,
    Recap = 4,
    Completed = 5,
}
