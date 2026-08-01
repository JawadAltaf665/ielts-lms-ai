namespace IELTSLms.Api.Contracts;

public sealed record GetExamHistoryQueryParams(string? Cursor, int? Limit);
