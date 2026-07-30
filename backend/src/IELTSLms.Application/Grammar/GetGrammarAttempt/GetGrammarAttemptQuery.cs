using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.SubmitGrammarAttempt;

namespace IELTSLms.Application.Grammar.GetGrammarAttempt;

/// <summary>
/// Gets a past attempt and its result, scoped to the caller's own attempt. The sprint's endpoint
/// table also allows a teacher with <c>analytics.view.cohort</c> to view a learner's attempt, but no
/// permission/cohort infrastructure exists yet anywhere in this codebase (roles are the only
/// enforced authorization primitive today) — ponytail: deferred until cohort/permission-based
/// authorization actually lands; for now this is strictly self-service.
/// </summary>
public sealed record GetGrammarAttemptQuery(Guid TenantId, Guid UserId, Guid AttemptId) : IQuery<GrammarAttemptResponse>;
