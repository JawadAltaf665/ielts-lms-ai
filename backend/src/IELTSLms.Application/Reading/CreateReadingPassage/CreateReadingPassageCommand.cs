using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Reading.CreateReadingPassage;

/// <summary>
/// Creates a tenant-authored reading passage (ERD ENT-021). Content-manager/admin only per the
/// sprint's permission matrix (<c>reading.manage</c>) — declared via <see cref="IAuthorizeRequest"/>
/// for documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, which only
/// checks <c>RequiredRoles</c> until fine-grained permission enforcement lands (mirrors Grammar's
/// <c>CreateGrammarTopicCommand</c>, Phase 6 Sprint 1).
/// </summary>
public sealed record CreateReadingPassageCommand(
    Guid TenantId, string Title, string PassageText, ReadingTrack Track, ReadingDifficulty Difficulty, Guid? CategoryId,
    int DurationSeconds) : ICommand<ReadingPassageResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["reading.manage"];
}

public sealed record ReadingPassageResponse(
    Guid Id, Guid? TenantId, string Title, string PassageText, string Track, string Difficulty, Guid? CategoryId,
    int DurationSeconds, bool IsGlobal);
