namespace IELTSLms.Api.Contracts;

public sealed record CreateReadingPassageRequest(
    string Title, string PassageText, string Track, string Difficulty, Guid? CategoryId, int DurationSeconds);

public sealed record UpdateReadingPassageRequest(
    string Title, string PassageText, string Track, string Difficulty, Guid? CategoryId, int DurationSeconds);

public sealed record GetReadingPassagesQueryParams(string? Track, string? Difficulty, int? Page, int? PageSize);

public sealed record AddReadingQuestionRequest(string Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence);

public sealed record StartReadingAttemptRequest(Guid PassageId);

public sealed record SubmitReadingAttemptRequest(IReadOnlyList<ReadingAnswerSubmissionRequest> Answers);

public sealed record ReadingAnswerSubmissionRequest(Guid QuestionId, string? Answer);
