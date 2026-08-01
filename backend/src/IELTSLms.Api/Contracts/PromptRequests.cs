namespace IELTSLms.Api.Contracts;

public sealed record CreatePromptVersionRequest(string Feature, string Scope, string SystemTemplate, Guid? AiProviderId);

public sealed record RollbackPromptRequest(string Reason);

public sealed record ListPromptTemplatesQueryParams(string? Feature, string? Scope, bool? IsActive, int? Page, int? PageSize);
