namespace IELTSLms.Api.Contracts;

public sealed record CreateWritingTaskRequest(
    string Title, string TaskType, string Track, string PromptText, string? RubricReference, int MinWordCount,
    int MaxWordCount, Guid? CategoryId);

public sealed record GetWritingTasksQueryParams(string? Track, string? TaskType, int? Page, int? PageSize);

/// <summary>No word-count field — the server always recomputes it from <see cref="ResponseText"/>.</summary>
public sealed record SubmitWritingRequest(Guid WritingTaskId, string ResponseText);

public sealed record OverrideWritingEvaluationRequest(
    decimal TaskResponse, decimal CoherenceCohesion, decimal LexicalResource, decimal GrammaticalRangeAccuracy, string Reason);

public sealed record GetWritingReviewQueueQueryParams(string? Cursor, int? Limit);

public sealed record ResolveWritingEvaluationReviewRequest(string Decision);
