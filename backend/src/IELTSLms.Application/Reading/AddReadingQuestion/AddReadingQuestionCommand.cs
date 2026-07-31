using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Reading.AddReadingQuestion;

/// <summary>Adds one question to a tenant-owned passage. <c>reading.manage</c> only.</summary>
public sealed record AddReadingQuestionCommand(
    Guid TenantId, Guid PassageId, ReadingQuestionType Type, string Prompt, IReadOnlyList<string> AnswerKey, int Sequence)
    : ICommand<ReadingQuestionResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.manage"];
}

public sealed record ReadingQuestionResponse(Guid Id, Guid PassageId, string Type, string Prompt, int Sequence);
