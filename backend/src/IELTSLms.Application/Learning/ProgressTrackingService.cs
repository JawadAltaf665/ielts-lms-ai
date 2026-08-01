using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Time;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Learning;

/// <summary>
/// Commits its own unit of work: callers are post-commit domain-event handlers, which run after the
/// triggering aggregate's transaction has already completed (ERD ENT-055: "idempotent, eventual
/// consistency" is by design, not an afterthought).
/// </summary>
public sealed class ProgressTrackingService(IProgressTrackingRepository repository, IUnitOfWork unitOfWork, IGuidProvider guids)
    : IProgressTrackingService
{
    public async Task RecordSkillActivityAsync(
        Guid tenantId, Guid userId, LearningSkill skill, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        var progress = await repository.GetByUserAndSkillAsync(tenantId, userId, skill, cancellationToken).ConfigureAwait(false);
        if (progress is null)
        {
            progress = ProgressTracking.Create(ProgressTrackingId.From(guids.NewGuid()), tenantId, userId, skill, nowUtc);
            repository.Add(progress);
        }

        progress.RecordActivity(nowUtc);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
