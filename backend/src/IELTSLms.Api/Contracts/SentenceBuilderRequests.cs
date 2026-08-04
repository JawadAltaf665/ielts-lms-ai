namespace IELTSLms.Api.Contracts;

public sealed record CreateSentenceExerciseRequest(string Prompt, IReadOnlyList<string> TargetPattern, string Difficulty);

public sealed record GetSentenceExercisesQueryParams(string? Difficulty, int? Page, int? PageSize);

public sealed record SubmitSentenceAttemptRequest(string SubmittedText);
