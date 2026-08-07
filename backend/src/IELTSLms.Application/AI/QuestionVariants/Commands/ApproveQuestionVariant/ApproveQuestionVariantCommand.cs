using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.ApproveQuestionVariant;

/// <summary>
/// Approves a draft — promotes it into a real, publishable question by delegating to the existing
/// <c>AddReadingQuestionCommand</c>/<c>AddListeningQuestionCommand</c> (no second, parallel
/// content-creation path, per Sprint 8's business rules). <c>content.manage</c> only.
/// </summary>
public sealed record ApproveQuestionVariantCommand(Guid TenantId, Guid DraftId) : ICommand<QuestionVariantResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["content.manage"];
}
