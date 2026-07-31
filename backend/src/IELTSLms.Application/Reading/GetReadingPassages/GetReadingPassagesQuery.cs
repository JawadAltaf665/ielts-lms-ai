using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Reading.CreateReadingPassage;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Reading;

namespace IELTSLms.Application.Reading.GetReadingPassages;

/// <summary>Lists reading passages visible to the caller's tenant (global + tenant-authored), filterable by track/difficulty.</summary>
public sealed record GetReadingPassagesQuery(Guid TenantId, ReadingTrack? Track, ReadingDifficulty? Difficulty, int Page, int PageSize)
    : IQuery<PagedList<ReadingPassageResponse>>;
