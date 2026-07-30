using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.RequestGrammarCoaching;

/// <summary>
/// Ad-hoc grammar check on arbitrary learner text (AI Architecture §13 Grammar Coach). Entitlement-
/// and quota-gated per INV-03 before enqueueing; the actual gateway call happens asynchronously in
/// <see cref="Jobs.GrammarCoachingJob"/> — declared via <see cref="IAuthorizeRequest"/> for
/// documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, which only checks
/// <c>RequiredRoles</c> (see that type's own remarks) — real gating here is the entitlement/quota check
/// in the handler, not RBAC.
/// </summary>
public sealed record RequestGrammarCoachingCommand(Guid TenantId, Guid UserId, string Text)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.grammar-coach.use"];
}

public sealed record AiOperationAcceptedResponse(Guid Id, string Status);
