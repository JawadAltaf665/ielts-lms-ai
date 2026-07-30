using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Grammar.GetGrammarProgress;

/// <summary>
/// Progress summary for a learner across grammar topics. Restricted to the caller viewing their own
/// progress until cohort/permission-based authorization exists (see the same note on
/// <c>GetGrammarAttemptQuery</c>) — a mismatched <paramref name="RequestedUserId"/> is rejected as
/// not found rather than forbidden, consistent with this codebase's cross-tenant 404 convention.
/// </summary>
public sealed record GetGrammarProgressQuery(Guid TenantId, Guid CallerUserId, Guid RequestedUserId)
    : IQuery<IReadOnlyList<GrammarProgressResponse>>;

public sealed record GrammarProgressResponse(Guid TopicId, string MasteryLevel, int AttemptCount, DateTimeOffset? LastAttemptedAtUtc);
