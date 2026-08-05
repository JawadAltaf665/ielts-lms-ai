using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.QuestionVariants;

internal static class QuestionVariantErrors
{
    public static readonly Error SourceQuestionNotFound = Error.NotFound(
        "NOT_FOUND_QUESTION_VARIANT_SOURCE", "The source question was not found on this passage/audio.");

    public static readonly Error DraftNotFound = Error.NotFound(
        "NOT_FOUND_QUESTION_VARIANT_DRAFT", "Question variant draft not found.");

    /// <summary>Mirrors <c>Domain.AI.QuestionVariantDraft</c>'s own <c>NotPending</c> error (same code) —
    /// duplicated here because Approve's promote-before-transition order (see
    /// <c>ApproveQuestionVariantCommandHandler</c>'s remarks) needs to short-circuit before calling the
    /// domain method at all, not just surface its result.</summary>
    public static readonly Error DraftNotPending = Error.Conflict(
        "CONFLICT_QUESTION_VARIANT_NOT_PENDING", "Only a pending draft can be approved or rejected.");
}
