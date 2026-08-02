namespace IELTSLms.Api.Contracts;

public sealed record CreateSpeakingTaskRequest(string Title, string Part, string PromptText, Guid? CategoryId);

public sealed record GetSpeakingTasksQueryParams(string? Part, int? Page, int? PageSize);

public sealed record OverrideSpeakingEvaluationRequest(
    decimal FluencyCoherence, decimal LexicalResource, decimal GrammaticalRangeAccuracy, decimal Pronunciation, string Reason);

public sealed record GetSpeakingReviewQueueQueryParams(string? Cursor, int? Limit);

public sealed record ResolveSpeakingEvaluationReviewRequest(string Decision);
