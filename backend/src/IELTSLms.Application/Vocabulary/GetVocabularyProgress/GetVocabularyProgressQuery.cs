using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.GradeVocabularyReview;

namespace IELTSLms.Application.Vocabulary.GetVocabularyProgress;

/// <summary>
/// Progress summary for a learner across vocabulary words. Restricted to the caller viewing their own
/// progress until cohort/permission-based authorization exists (see the same note on
/// <c>GetGrammarProgressQuery</c>) — a mismatched <paramref name="RequestedUserId"/> is rejected as not
/// found rather than forbidden, consistent with this codebase's cross-tenant 404 convention.
/// </summary>
public sealed record GetVocabularyProgressQuery(Guid TenantId, Guid CallerUserId, Guid RequestedUserId)
    : IQuery<IReadOnlyList<VocabularyProgressResponse>>;
