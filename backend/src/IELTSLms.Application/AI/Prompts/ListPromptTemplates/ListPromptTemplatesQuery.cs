using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.AI.Prompts.ListPromptTemplates;

/// <summary>Lists the caller's tenant-scoped prompt templates, filterable by feature/scope/active status.</summary>
public sealed record ListPromptTemplatesQuery(Guid TenantId, string? Feature, string? Scope, bool? IsActive, int Page, int PageSize)
    : IQuery<PagedList<AiPromptResponse>>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
