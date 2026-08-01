using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Speaking;

/// <summary>
/// A Speaking task prompt (ERD ENT-030): part (1-3) plus prompt content. <see cref="TenantId"/> is
/// nullable: <see langword="null"/> for a global-catalog task authored once and visible to every
/// tenant (read-only to tenants), non-null for a tenant's own authored task — same global-vs-tenant
/// split as <c>WritingTask</c>/<c>ReadingPassage</c>/<c>ListeningAudio</c>. Exempt from the automatic
/// tenant query filter for the same reason; handlers must filter explicitly via
/// <c>ISpeakingTaskRepository.GetVisibleByIdAsync</c>/<c>ListVisibleAsync</c>.
/// </summary>
public sealed class SpeakingTask : AuditableEntity<SpeakingTaskId>
{
    private SpeakingTask(
        SpeakingTaskId id, Guid? tenantId, string title, SpeakingPart part, string promptText, Guid? categoryId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Part = part;
        PromptText = promptText;
        CategoryId = categoryId;
        CreatedAtUtc = nowUtc;
    }

    private SpeakingTask()
    {
        Title = null!;
        PromptText = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog task.</summary>
    public Guid? TenantId { get; private init; }

    public string Title { get; private set; }

    public SpeakingPart Part { get; private set; }

    public string PromptText { get; private set; }

    public Guid? CategoryId { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<SpeakingTask> Create(
        SpeakingTaskId id, Guid? tenantId, string title, SpeakingPart part, string promptText, Guid? categoryId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<SpeakingTask>(SpeakingDomainErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(promptText))
        {
            return Result.Failure<SpeakingTask>(SpeakingDomainErrors.PromptTextRequired);
        }

        return Result.Success(new SpeakingTask(id, tenantId, title, part, promptText, categoryId, nowUtc));
    }
}

internal static class SpeakingDomainErrors
{
    public static readonly Error TitleRequired = Error.Validation(
        "VALIDATION_SPEAKING_TITLE_REQUIRED", "Title is required.");

    public static readonly Error PromptTextRequired = Error.Validation(
        "VALIDATION_SPEAKING_PROMPT_TEXT_REQUIRED", "Prompt text is required.");

    public static readonly Error DurationInvalid = Error.Validation(
        "VALIDATION_SPEAKING_DURATION_INVALID", "Duration in seconds must be positive.");

    public static readonly Error RubricScoreOutOfRange = Error.Validation(
        "VALIDATION_SPEAKING_RUBRIC_SCORE_OUT_OF_RANGE", "Rubric criterion scores must be between 0 and 9 in 0.5 increments.");

    public static readonly Error ConfidenceOutOfRange = Error.Validation(
        "VALIDATION_SPEAKING_CONFIDENCE_OUT_OF_RANGE", "Confidence must be between 0 and 1.");
}
