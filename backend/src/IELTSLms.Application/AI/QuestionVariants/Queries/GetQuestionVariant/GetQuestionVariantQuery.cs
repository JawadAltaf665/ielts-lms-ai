using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.QuestionVariants.Queries.GetQuestionVariant;

/// <summary>Gets a draft variant. <c>content.manage</c> only.</summary>
public sealed record GetQuestionVariantQuery(Guid TenantId, Guid DraftId) : IQuery<QuestionVariantResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["content.manage"];
}
