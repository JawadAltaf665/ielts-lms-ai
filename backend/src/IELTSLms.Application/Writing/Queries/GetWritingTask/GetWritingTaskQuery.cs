using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Commands.CreateWritingTask;

namespace IELTSLms.Application.Writing.Queries.GetWritingTask;

public sealed record GetWritingTaskQuery(Guid TenantId, Guid TaskId) : IQuery<WritingTaskResponse>;
