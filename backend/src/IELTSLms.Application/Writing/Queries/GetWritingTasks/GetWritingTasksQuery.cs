using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Writing.Queries.GetWritingTasks;

/// <summary>Lists writing tasks visible to the caller's tenant (global + tenant-authored), filterable by track/taskType.</summary>
public sealed record GetWritingTasksQuery(Guid TenantId, ReadingTrack? Track, WritingTaskType? TaskType, int Page, int PageSize)
    : IQuery<PagedList<WritingTaskResponse>>;
