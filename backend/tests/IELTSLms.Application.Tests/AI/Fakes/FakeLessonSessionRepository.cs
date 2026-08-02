using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeLessonSessionRepository : ILessonSessionRepository
{
    private readonly List<LessonSession> _sessions = [];

    public void Seed(LessonSession session) => _sessions.Add(session);

    public Task<LessonSession?> GetByIdAsync(LessonSessionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.FirstOrDefault(s => s.Id == id));

    public void Add(LessonSession aggregate) => _sessions.Add(aggregate);

    public void Remove(LessonSession aggregate) => _sessions.Remove(aggregate);

    public Task<LessonSession?> GetActiveByUserAndLessonAsync(
        Guid tenantId, Guid userId, Guid lessonId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_sessions.FirstOrDefault(
            s => s.TenantId == tenantId && s.UserId == userId && s.LessonId == lessonId && s.CurrentStep != LessonStep.Completed));
}
