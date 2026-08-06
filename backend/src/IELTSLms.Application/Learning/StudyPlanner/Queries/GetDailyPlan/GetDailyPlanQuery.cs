using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Learning.StudyPlanner.Queries.GetDailyPlan;

public sealed record GetDailyPlanQuery(Guid TenantId, Guid UserId) : IQuery<DailyPlanResponse>;

public sealed record DailyPlanResponse(Guid DailyPlanId, DateOnly PlanDate, IReadOnlyList<DailyPlanItemDto> Items);
