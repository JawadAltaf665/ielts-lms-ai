using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.RequestVocabularyCoaching;

/// <summary>
/// Usage/synonym/collocation suggestions for a word, optionally scoped by topic/difficulty (AI
/// Architecture §13 capability catalog). Entitlement- and quota-gated per INV-03 before enqueueing —
/// same shape as <see cref="RequestGrammarCoachingCommand"/>; the actual gateway call happens
/// asynchronously in <see cref="Jobs.VocabularyCoachingJob"/>.
/// </summary>
public sealed record RequestVocabularyCoachingCommand(Guid TenantId, Guid UserId, string Word, string? Topic, string? Difficulty)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["ai.vocabulary-coach.use"];
}
