using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// A draft, AI-generated variant of an existing Reading/Listening question — output of the
/// difficulty-adaptation capability (Sprint 8, AI Architecture §13 "generation assists, humans
/// approve"). <see cref="SourceQuestionId"/> is a polymorphic reference (<see cref="SourceType"/> picks
/// which bounded context's question table it points at, same shape as
/// <see cref="AiResponse.RequestingEntityType"/>/<see cref="AiResponse.RequestingEntityId"/>).
/// <see cref="DraftContent"/> is inert until a <c>content.manage</c> actor calls <see cref="Approve"/> —
/// approval promotes it into a real question through the existing Reading/Listening content-authoring
/// commands (Application's job, not this entity's); this row is never auto-published. A rejected draft
/// keeps its row (<see cref="Reject"/> only flips <see cref="Status"/>) for audit, mirroring evaluation-
/// override decisions elsewhere in the platform.
/// </summary>
public sealed class QuestionVariantDraft : TenantAuditableEntity<QuestionVariantDraftId>
{
    private QuestionVariantDraft(
        QuestionVariantDraftId id, Guid tenantId, QuestionVariantSourceType sourceType, Guid sourceQuestionId,
        string draftContent, int difficultyDelta, AiResponseId aiResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        SourceType = sourceType;
        SourceQuestionId = sourceQuestionId;
        DraftContent = draftContent;
        DifficultyDelta = difficultyDelta;
        Status = QuestionVariantStatus.Pending;
        AiResponseId = aiResponseId;
        CreatedAtUtc = nowUtc;
    }

    private QuestionVariantDraft()
    {
        DraftContent = null!;
    }

    public QuestionVariantSourceType SourceType { get; private init; }

    public Guid SourceQuestionId { get; private init; }

    /// <summary>Structured JSON: the same <c>{ type, prompt, answerKey }</c> shape a Reading/Listening
    /// question is authored with — see <c>QuestionVariantContent</c> (Application).</summary>
    public string DraftContent { get; private init; }

    /// <summary>Requested difficulty adjustment relative to the source question, e.g. <c>+1</c> harder.</summary>
    public int DifficultyDelta { get; private init; }

    public QuestionVariantStatus Status { get; private set; }

    public AiResponseId AiResponseId { get; private init; }

    public static Result<QuestionVariantDraft> Create(
        QuestionVariantDraftId id, Guid tenantId, QuestionVariantSourceType sourceType, Guid sourceQuestionId,
        string draftContent, int difficultyDelta, AiResponseId aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(draftContent))
        {
            return Result.Failure<QuestionVariantDraft>(QuestionVariantDraftDomainErrors.DraftContentRequired);
        }

        return Result.Success(new QuestionVariantDraft(
            id, tenantId, sourceType, sourceQuestionId, draftContent, difficultyDelta, aiResponseId, nowUtc));
    }

    public Result Approve(DateTimeOffset nowUtc)
    {
        if (Status != QuestionVariantStatus.Pending)
        {
            return Result.Failure(QuestionVariantDraftDomainErrors.NotPending);
        }

        Status = QuestionVariantStatus.Approved;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Reject(DateTimeOffset nowUtc)
    {
        if (Status != QuestionVariantStatus.Pending)
        {
            return Result.Failure(QuestionVariantDraftDomainErrors.NotPending);
        }

        Status = QuestionVariantStatus.Rejected;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class QuestionVariantDraftDomainErrors
{
    public static readonly Error DraftContentRequired = Error.Validation(
        "VALIDATION_QUESTION_VARIANT_DRAFT_CONTENT_REQUIRED", "Draft content is required.");

    public static readonly Error NotPending = Error.Conflict(
        "CONFLICT_QUESTION_VARIANT_NOT_PENDING", "Only a pending draft can be approved or rejected.");
}
