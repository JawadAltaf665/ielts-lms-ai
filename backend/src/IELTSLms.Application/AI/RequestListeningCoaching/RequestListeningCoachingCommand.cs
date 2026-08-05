using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.RequestListeningCoaching;

/// <summary>
/// On-demand Listening strategy guidance, and the delegate target for attempt-grounded auto-explanation
/// (AI Architecture §13 capability catalog). Mirrors <c>RequestReadingCoachingCommand</c> exactly.
/// </summary>
public sealed record RequestListeningCoachingCommand(Guid TenantId, Guid UserId, string Text)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.listening-coach.use"];
}
