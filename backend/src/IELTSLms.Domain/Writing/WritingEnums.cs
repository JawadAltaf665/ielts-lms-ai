namespace IELTSLms.Domain.Writing;

/// <summary>IELTS Writing task type (ERD ENT-027).</summary>
public enum WritingTaskType
{
    Task1 = 1,
    Task2 = 2,
}

/// <summary>Lifecycle of a <see cref="WritingSubmission"/> (ERD ENT-028).</summary>
public enum WritingSubmissionStatus
{
    Draft = 1,
    Submitted = 2,
    Evaluated = 3,
    Failed = 4,
}

/// <summary>The four official IELTS Writing rubric criteria (DDD §8.3 <c>Evaluation</c> value object).</summary>
public enum WritingRubricCriterion
{
    TaskResponse = 1,
    CoherenceCohesion = 2,
    LexicalResource = 3,
    GrammaticalRangeAccuracy = 4,
}
