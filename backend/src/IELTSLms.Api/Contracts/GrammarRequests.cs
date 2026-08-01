namespace IELTSLms.Api.Contracts;

public sealed record CreateGrammarTopicRequest(string Title, string Description, string Track, string Difficulty, Guid? CategoryId);

public sealed record UpdateGrammarTopicRequest(string Title, string Description, string Track, string Difficulty, Guid? CategoryId);

public sealed record GetGrammarTopicsQueryParams(string? Track, string? Difficulty, int? Page, int? PageSize);

public sealed record CreateGrammarExerciseRequest(
    string Title, string Mode, int Order, IReadOnlyList<CreateGrammarQuestionItemRequest> Questions);

public sealed record CreateGrammarQuestionItemRequest(string Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence);

public sealed record SubmitGrammarAttemptRequest(IReadOnlyList<GrammarAnswerSubmissionRequest> Answers);

public sealed record GrammarAnswerSubmissionRequest(Guid QuestionId, string? Answer);
