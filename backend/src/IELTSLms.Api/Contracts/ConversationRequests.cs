namespace IELTSLms.Api.Contracts;

public sealed record ListConversationsQueryParams(string? Feature, string? Status, string? Cursor, int? Limit);

public sealed record GetConversationQueryParams(string? Cursor, int? Limit);
