using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Writing;

/// <summary>
/// A Writing task prompt plus its authored metadata (ERD ENT-027): task type, track, prompt text, and
/// the server-configured word-count bounds a <see cref="WritingSubmission"/> is validated against.
/// <see cref="TenantId"/> is nullable: <see langword="null"/> for a global-catalog task authored once
/// and visible to every tenant (read-only to tenants), non-null for a tenant's own authored task —
/// same global-vs-tenant split as <c>ReadingPassage</c>/<c>ListeningAudio</c>/<c>GrammarTopic</c>.
/// Because of the nullable tenant this does not implement <see cref="ITenantScoped"/>, so it is exempt
/// from the automatic tenant query filter; handlers must filter explicitly (global OR caller's tenant)
/// via <c>IWritingTaskRepository.GetVisibleByIdAsync</c>/<c>ListVisibleAsync</c>.
/// </summary>
public sealed class WritingTask : AuditableEntity<WritingTaskId>
{
    private WritingTask(
        WritingTaskId id, Guid? tenantId, string title, WritingTaskType taskType, ReadingTrack track, string promptText,
        string? rubricReference, int minWordCount, int maxWordCount, Guid? categoryId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        TaskType = taskType;
        Track = track;
        PromptText = promptText;
        RubricReference = rubricReference;
        MinWordCount = minWordCount;
        MaxWordCount = maxWordCount;
        CategoryId = categoryId;
        CreatedAtUtc = nowUtc;
    }

    private WritingTask()
    {
        Title = null!;
        PromptText = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog task.</summary>
    public Guid? TenantId { get; private init; }

    public string Title { get; private set; }

    public WritingTaskType TaskType { get; private set; }

    public ReadingTrack Track { get; private set; }

    public string PromptText { get; private set; }

    public string? RubricReference { get; private set; }

    /// <summary>Server-configured lower bound a submission's recomputed word count must meet. Never client-supplied.</summary>
    public int MinWordCount { get; private set; }

    /// <summary>Server-configured upper bound a submission's recomputed word count must meet. Never client-supplied.</summary>
    public int MaxWordCount { get; private set; }

    public Guid? CategoryId { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<WritingTask> Create(
        WritingTaskId id, Guid? tenantId, string title, WritingTaskType taskType, ReadingTrack track, string promptText,
        string? rubricReference, int minWordCount, int maxWordCount, Guid? categoryId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<WritingTask>(WritingDomainErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(promptText))
        {
            return Result.Failure<WritingTask>(WritingDomainErrors.PromptTextRequired);
        }

        if (minWordCount <= 0 || maxWordCount <= 0 || minWordCount > maxWordCount)
        {
            return Result.Failure<WritingTask>(WritingDomainErrors.WordCountBoundsInvalid);
        }

        return Result.Success(new WritingTask(
            id, tenantId, title, taskType, track, promptText, rubricReference, minWordCount, maxWordCount, categoryId, nowUtc));
    }
}

internal static class WritingDomainErrors
{
    public static readonly Error TitleRequired = Error.Validation(
        "VALIDATION_WRITING_TITLE_REQUIRED", "Title is required.");

    public static readonly Error PromptTextRequired = Error.Validation(
        "VALIDATION_WRITING_PROMPT_TEXT_REQUIRED", "Prompt text is required.");

    public static readonly Error WordCountBoundsInvalid = Error.Validation(
        "VALIDATION_WRITING_WORD_COUNT_BOUNDS_INVALID", "MinWordCount and MaxWordCount must be positive, with MinWordCount not exceeding MaxWordCount.");

    public static readonly Error ResponseTextRequired = Error.Validation(
        "VALIDATION_WRITING_RESPONSE_TEXT_REQUIRED", "Response text is required.");

    public static readonly Error RubricScoreOutOfRange = Error.Validation(
        "VALIDATION_WRITING_RUBRIC_SCORE_OUT_OF_RANGE", "Rubric criterion scores must be between 0 and 9 in 0.5 increments.");

    public static readonly Error ConfidenceOutOfRange = Error.Validation(
        "VALIDATION_WRITING_CONFIDENCE_OUT_OF_RANGE", "Confidence must be between 0 and 1.");

    public static readonly Error SimilarityScoreOutOfRange = Error.Validation(
        "VALIDATION_WRITING_SIMILARITY_SCORE_OUT_OF_RANGE", "Similarity score must be between 0 and 1.");

    public static readonly Error ReviewDecisionRequired = Error.Validation(
        "VALIDATION_WRITING_REVIEW_DECISION_REQUIRED", "A review decision is required.");

    public static readonly Error EvaluationNotFlaggedForReview = Error.Conflict(
        "CONFLICT_WRITING_EVALUATION_NOT_FLAGGED_FOR_REVIEW", "This evaluation is not flagged for human review.");
}
