using IELTSLms.Domain.Learning.Events;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Learning;

/// <summary>
/// Canonical per-learner, per-skill progress record (ERD ENT-055; DDD §13.4 Learning Management,
/// BC-04). Every skill module reports activity here through <c>IProgressTrackingService</c> instead
/// of maintaining its own cross-module rollup — one row per (<see cref="UserId"/>, <see cref="Skill"/>).
/// Written from post-commit domain-event handlers, so updates are idempotent and eventually
/// consistent by design (ERD §6.9).
/// </summary>
public sealed class ProgressTracking : TenantAuditableEntity<ProgressTrackingId>
{
    private ProgressTracking(ProgressTrackingId id, Guid tenantId, Guid userId, LearningSkill skill, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Skill = skill;
        ActivityCount = 0;
        CreatedAtUtc = nowUtc;
    }

    private ProgressTracking()
    {
    }

    public Guid UserId { get; private set; }

    public LearningSkill Skill { get; private set; }

    /// <summary>Total number of completed skill activities recorded (attempts, submissions, etc.).</summary>
    public int ActivityCount { get; private set; }

    public DateTimeOffset? LastActivityAtUtc { get; private set; }

    public static ProgressTracking Create(ProgressTrackingId id, Guid tenantId, Guid userId, LearningSkill skill, DateTimeOffset nowUtc) =>
        new(id, tenantId, userId, skill, nowUtc);

    /// <summary>Records one completed activity for this skill. Idempotent to call once per activity.
    /// Raises <see cref="ProgressRecalculatedDomainEvent"/> (POL-06) so certificate eligibility gets
    /// (re)evaluated off the back of every skill activity, not just mock-exam completion.</summary>
    public void RecordActivity(DateTimeOffset nowUtc)
    {
        ActivityCount++;
        LastActivityAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new ProgressRecalculatedDomainEvent(TenantId, UserId, Skill));
    }
}
