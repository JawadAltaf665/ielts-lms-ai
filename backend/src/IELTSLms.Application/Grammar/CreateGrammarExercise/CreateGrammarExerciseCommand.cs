using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Grammar.CreateGrammarExercise;

/// <summary>
/// Creates an exercise under a topic together with its full question set (there is no separate
/// question-authoring endpoint this sprint — content managers author a drill as one atomic unit).
/// <c>grammar.manage</c> only.
/// </summary>
public sealed record CreateGrammarExerciseCommand(
    Guid TenantId, Guid TopicId, string Title, GrammarExerciseMode Mode, int Order,
    IReadOnlyList<CreateGrammarQuestionRequest> Questions) : ICommand<GrammarExerciseResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["grammar.manage"];
}

public sealed record CreateGrammarQuestionRequest(
    GrammarQuestionType Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence);

public sealed record GrammarExerciseResponse(Guid Id, Guid TopicId, string Title, string Mode, int Order, int QuestionCount);
