using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Listening.CreateListeningAudio;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Listening.GetListeningAudios;

/// <summary>Lists listening audios visible to the caller's tenant (global + tenant-authored), filterable by track.</summary>
public sealed record GetListeningAudiosQuery(Guid TenantId, ReadingTrack? Track, int Page, int PageSize)
    : IQuery<PagedList<ListeningAudioResponse>>;
