namespace IELTSLms.Domain.Learning;

/// <summary>
/// The skill dimension a <see cref="ProgressTracking"/> row tracks (ERD ENT-055; DDD §13.4). Every
/// skill bounded context (Grammar first, then Vocabulary/Reading/Listening/Writing/Speaking/Sentence
/// Builder) reports activity under one of these values through <c>IProgressTrackingService</c> — this
/// list grows as later sprints add skill modules, never per-module ad hoc strings.
/// </summary>
public enum LearningSkill
{
    Grammar = 1,
    Vocabulary = 2,
    SentenceBuilder = 3,
    Reading = 4,
    Listening = 5,
    Writing = 6,
    Speaking = 7,

    /// <summary>AI Teacher structured lesson delivery (Phase 7 Sprint 4) — recorded when a
    /// <c>LessonSession</c> completes, alongside the human-authored skill modules above.</summary>
    AiTeacher = 8,
}

/// <summary>Lifecycle of a <see cref="LearningRecommendation"/> set (Sprint 9, Agentic AI Architecture
/// §13.7): a freshly generated set is <see cref="Active"/> until either a newer set is generated
/// (<see cref="Superseded"/>) or the learner dismisses it (<see cref="Dismissed"/>) — both keep the row
/// for audit/history rather than deleting it.</summary>
public enum LearningRecommendationStatus
{
    Active = 1,
    Superseded = 2,
    Dismissed = 3,
}
