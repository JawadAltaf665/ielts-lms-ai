using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Listening.AddListeningQuestion;

/// <summary>Adds one question to a tenant-owned audio. Reuses <see cref="ReadingQuestionType"/> (no parallel enum). <c>listening.manage</c> only.</summary>
public sealed record AddListeningQuestionCommand(
    Guid TenantId, Guid AudioId, ReadingQuestionType Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence)
    : ICommand<ListeningQuestionResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.manage"];
}

public sealed record ListeningQuestionResponse(Guid Id, Guid AudioId, string Type, string Prompt, int Sequence);
