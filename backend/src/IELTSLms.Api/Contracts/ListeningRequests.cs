namespace IELTSLms.Api.Contracts;

public sealed record GetListeningAudiosQueryParams(string? Track, int? Page, int? PageSize);

public sealed record AddListeningQuestionRequest(string Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence);

public sealed record StartListeningAttemptRequest(Guid AudioId, string Mode);

public sealed record SubmitListeningAttemptRequest(IReadOnlyList<ListeningAnswerSubmissionRequest> Answers);

public sealed record ListeningAnswerSubmissionRequest(Guid QuestionId, string? Answer);
