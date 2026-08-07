using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.RejectQuestionVariant;

/// <summary>Rejects a draft. The row is retained (status, not hard-deleted) for audit. <c>content.manage</c> only.</summary>
public sealed record RejectQuestionVariantCommand(Guid TenantId, Guid DraftId) : ICommand<QuestionVariantResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["content.manage"];
}
