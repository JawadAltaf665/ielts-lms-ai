using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingTask;

public sealed record GetSpeakingTaskQuery(Guid TenantId, Guid TaskId) : IQuery<SpeakingTaskResponse>;
