using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Prompts.GetPromptTemplate;

public sealed record GetPromptTemplateQuery(Guid TenantId, Guid PromptId) : IQuery<AiPromptResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
