using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;

namespace IELTSLms.Application.Writing.Commands.OverrideWritingEvaluation;

/// <summary>
/// A teacher override of the AI-produced rubric scores (sprint requirement: audited, original AI
/// scores retained — see <see cref="Domain.Writing.WritingEvaluation.Override"/>). Teacher-only,
/// checked manually in the handler via <c>ICurrentUserService.Roles</c> (there is no cohort/enrollment
/// concept in this codebase yet, so this is tenant-wide, not scoped to a specific cohort — see the
/// sprint's scope-cut note) — <see cref="IAuthorizeRequest"/> is documentation-only here, same
/// established gap as every other fine-grained permission in this codebase.
/// </summary>
public sealed record OverrideWritingEvaluationCommand(
    Guid TenantId, Guid OverriddenBy, Guid EvaluationId, decimal TaskResponse, decimal CoherenceCohesion,
    decimal LexicalResource, decimal GrammaticalRangeAccuracy, string Reason) : ICommand<WritingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.evaluate"];
}
