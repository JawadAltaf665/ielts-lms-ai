using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;

namespace IELTSLms.Application.Speaking.Commands.OverrideSpeakingEvaluation;

/// <summary>
/// A teacher override of the AI-produced rubric scores (audited, original AI scores retained — see
/// <see cref="Domain.Speaking.SpeakingEvaluation.Override"/>). Teacher-only, checked manually in the
/// handler via <c>ICurrentUserService.Roles</c> — <see cref="IAuthorizeRequest"/> is documentation-only
/// here, same established gap as every other fine-grained permission in this codebase (mirrors
/// <c>OverrideWritingEvaluationCommand</c>).
/// </summary>
public sealed record OverrideSpeakingEvaluationCommand(
    Guid TenantId, Guid OverriddenBy, Guid EvaluationId, decimal FluencyCoherence, decimal LexicalResource,
    decimal GrammaticalRangeAccuracy, decimal Pronunciation, string Reason)
    : ICommand<SpeakingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.evaluate"];
}
