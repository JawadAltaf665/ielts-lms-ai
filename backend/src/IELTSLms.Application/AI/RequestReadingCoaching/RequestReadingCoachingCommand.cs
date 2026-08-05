using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.RequestReadingCoaching;

/// <summary>
/// On-demand Reading strategy guidance, and the delegate target for attempt-grounded auto-explanation
/// (AI Architecture §13 capability catalog). Entitlement- and quota-gated per INV-03 before enqueueing
/// — same shape as <see cref="RequestGrammarCoachingCommand"/>; the actual gateway call happens
/// asynchronously in <see cref="Jobs.ReadingCoachingJob"/>.
/// </summary>
public sealed record RequestReadingCoachingCommand(Guid TenantId, Guid UserId, string Text)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.reading-coach.use"];
}
