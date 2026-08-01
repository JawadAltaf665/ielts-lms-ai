using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Listening.CreateListeningAudio;

/// <summary>
/// Creates a tenant-authored listening audio (ERD ENT-024) plus its backing <c>FileAsset</c>
/// (ENT-057) in one step — the multipart upload flow from the sprint's endpoint table ("multipart or
/// pre-signed upload flow"; multipart chosen for simplicity — ponytail: add a separate pre-signed-PUT
/// flow only if direct-to-storage upload becomes a real requirement). Content-manager/admin only
/// (<c>listening.manage</c>), same tenant-ownership rule as <c>CreateReadingPassageCommand</c>
/// (Phase 6 Sprint 4): global-catalog audios are seeded content, never authored through this endpoint.
/// </summary>
public sealed record CreateListeningAudioCommand(
    Guid TenantId, Guid UserId, string Title, ReadingTrack Track, int DurationSeconds, Guid? CategoryId,
    string? TranscriptText, Stream FileContent, string FileName, string ContentType, long SizeBytes)
    : ICommand<ListeningAudioResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.manage"];
}

public sealed record ListeningAudioResponse(
    Guid Id, Guid? TenantId, string Title, string Track, Guid? CategoryId, int DurationSeconds, bool IsGlobal);
