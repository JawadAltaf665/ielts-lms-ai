using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Writing.Commands.CreateWritingTask;

/// <summary>
/// Creates a tenant-authored writing task (ERD ENT-027). Content-manager/teacher only per the
/// sprint's permission matrix (<c>writing.manage</c>) — declared via <see cref="IAuthorizeRequest"/>
/// for documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, which only
/// checks <c>RequiredRoles</c> until fine-grained permission enforcement lands (mirrors
/// <c>CreateReadingPassageCommand</c>).
/// </summary>
public sealed record CreateWritingTaskCommand(
    Guid TenantId, string Title, WritingTaskType TaskType, ReadingTrack Track, string PromptText, string? RubricReference,
    int MinWordCount, int MaxWordCount, Guid? CategoryId) : ICommand<WritingTaskResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.manage"];
}

public sealed record WritingTaskResponse(
    Guid Id, Guid? TenantId, string Title, string TaskType, string Track, string PromptText, string? RubricReference,
    int MinWordCount, int MaxWordCount, Guid? CategoryId, bool IsGlobal);
