using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Learning.StudyPlanner.Queries.GetDailyPlan;

namespace IELTSLms.Application.Learning.StudyPlanner.Commands.CompleteDailyPlanItem;

public sealed record CompleteDailyPlanItemCommand(Guid TenantId, Guid UserId, Guid ItemId) : ICommand<DailyPlanResponse>;
