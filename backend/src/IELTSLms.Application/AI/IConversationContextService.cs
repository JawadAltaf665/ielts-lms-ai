using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI;

/// <summary>
/// Assembles the bounded context for one conversational turn (Phase 7 Sprint 3, AI Architecture §6,
/// Agentic AI Architecture §5): the composed system prompt (via <see cref="IPromptCompositionService"/>)
/// plus the latest <see cref="ConversationSummary"/> (if any) and the raw messages since it, compacting
/// via one more bounded <see cref="Abstractions.AI.IAiGateway"/> call when the raw tail exceeds its token
/// budget. Shared, context-agnostic infrastructure other AI capabilities depend on — analogous in shape
/// to <c>IProgressTrackingService</c> — not owned or re-implemented by any one capability.
/// </summary>
public interface IConversationContextService
{
    /// <param name="tenantId">Tenant scope; a conversation outside it is reported as not found.</param>
    /// <param name="userId">Owning learner; a conversation owned by someone else is reported as not
    /// found (isolation as 404, never 403).</param>
    /// <param name="conversationId">The conversation to assemble context for.</param>
    /// <param name="prompt">The capability's resolved, active prompt template (Tutor/Teacher's own
    /// concern to resolve — this service composes it, it does not select it).</param>
    /// <param name="systemVariables">Trusted, server-assembled composition variables — see
    /// <see cref="IPromptCompositionService.Compose"/>.</param>
    /// <param name="userContent">The new turn's learner-facing content, passed through composition
    /// untouched.</param>
    Task<Result<AssembledConversationContext>> AssembleAsync(
        Guid tenantId,
        Guid userId,
        ConversationId conversationId,
        AiPrompt prompt,
        IReadOnlyDictionary<string, string> systemVariables,
        string userContent,
        CancellationToken cancellationToken = default);
}

/// <summary>The bounded context ready to hand to <see cref="Abstractions.AI.IAiGateway"/>: the composed
/// system/user pair plus prior turns within budget (latest summary, if any, first).</summary>
public sealed record AssembledConversationContext(string SystemPrompt, string UserContent, IReadOnlyList<ConversationContextTurn> History);

/// <summary>One prior turn in the assembled context — either a raw message or the summary standing in
/// for the turns it replaced (reported as <see cref="ConversationMessageRole.System"/>).</summary>
public sealed record ConversationContextTurn(ConversationMessageRole Role, string Content);
