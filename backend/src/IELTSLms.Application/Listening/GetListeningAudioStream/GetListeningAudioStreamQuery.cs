using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Listening.GetListeningAudioStream;

/// <summary>
/// Issues the initial, uncounted signed playback URL for an audio (API Design Standards §14.1). Every
/// subsequent replay goes through <c>RegisterListeningReplayCommand</c> instead, which enforces the
/// mode's server-tracked cap.
/// </summary>
public sealed record GetListeningAudioStreamQuery(Guid TenantId, Guid AudioId) : IQuery<ListeningStreamUrlResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["listening.attempt"];
}

public sealed record ListeningStreamUrlResponse(string Url, DateTimeOffset ExpiresAtUtc);
