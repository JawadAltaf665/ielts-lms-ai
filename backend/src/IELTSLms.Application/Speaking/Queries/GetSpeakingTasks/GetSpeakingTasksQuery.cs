using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingTasks;

/// <summary>Lists speaking tasks visible to the caller's tenant (global + tenant-authored), filterable by part.</summary>
public sealed record GetSpeakingTasksQuery(Guid TenantId, SpeakingPart? Part, int Page, int PageSize)
    : IQuery<PagedList<SpeakingTaskResponse>>;
