using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.GenerateQuestionVariants;

/// <summary>
/// Generates one draft, AI-adjusted variant of an existing Reading/Listening question (Sprint 8 scope
/// item 3) — the sole source of both <c>POST /reading-passages/{id}/question-variants</c> and
/// <c>POST /listening-audios/{id}/question-variants</c> (they differ only in <see cref="SourceType"/>),
/// same one-command-two-callers shape the sprint's own CQRS layout calls for. <c>content.manage</c>
/// only, no entitlement/quota (an authoring tool, not a metered learner capability — see
/// <c>AiFeatures.ReadingQuestionVariants</c>'s remarks). Returns 202 + an operation resource; the
/// resulting <see cref="Domain.AI.QuestionVariantDraft"/>, once generated, shares the same id as the
/// operation (see <see cref="Jobs.GenerateQuestionVariantsJob"/>'s remarks), so polling
/// <c>/operations/{id}</c> to success and then calling <c>GET /question-variants/{id}</c> with that
/// same id is the intended flow.
/// </summary>
public sealed record GenerateQuestionVariantsCommand(
    Guid TenantId, Guid UserId, QuestionVariantSourceType SourceType, Guid ContainerId, Guid SourceQuestionId, int DifficultyDelta)
    : ICommand<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["content.manage"];
}
