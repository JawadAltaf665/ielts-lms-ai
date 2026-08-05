using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.QuestionVariants;

/// <summary>Shared response shape for the generate/get/approve/reject endpoints — <see cref="Content"/>
/// is the parsed <see cref="QuestionVariantContent"/>, not the raw stored JSON string.</summary>
public sealed record QuestionVariantResponse(
    Guid Id, string SourceType, Guid SourceQuestionId, QuestionVariantContent Content, int DifficultyDelta, string Status)
{
    public static QuestionVariantResponse From(QuestionVariantDraft draft, QuestionVariantContent content) => new(
        draft.Id.Value, draft.SourceType.ToString(), draft.SourceQuestionId, content, draft.DifficultyDelta,
        draft.Status.ToString());
}
