using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Learning;

/// <summary>
/// A learner's current cross-skill weakness profile (Sprint 9; AI Architecture §13 "Weakness
/// Detection"; Agentic AI Architecture §12.2 Progress Analysis Agent). One row per
/// (<see cref="TenantId"/>, <see cref="UserId"/>) — <see cref="Recompute"/> overwrites it in place
/// rather than growing a history table, mirroring <see cref="ProgressTracking"/>'s "idempotent
/// updates, eventual consistency" posture (ERD §6.9). <see cref="SkillScoresJson"/> is the structured
/// per-skill breakdown (see <c>SkillWeaknessScoreDto</c>, Application layer) — stored as opaque JSON on
/// the domain entity, same convention as <see cref="AI.QuestionVariantDraft.DraftContent"/>.
/// <see cref="AiResponseId"/> is nullable per the ERD note: a future purely-statistical recompute path
/// would leave it unset.
/// </summary>
public sealed class WeaknessProfile : TenantAuditableEntity<WeaknessProfileId>
{
    private WeaknessProfile(
        WeaknessProfileId id, Guid tenantId, Guid userId, string skillScoresJson, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SkillScoresJson = skillScoresJson;
        AiResponseId = aiResponseId;
        ComputedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private WeaknessProfile()
    {
        SkillScoresJson = null!;
    }

    public Guid UserId { get; private init; }

    /// <summary>Structured JSON: a list of <c>{ skill, score, sampleSize }</c> — see
    /// <c>SkillWeaknessScoreDto</c> (Application). <c>score</c> is 0-100, higher = weaker.</summary>
    public string SkillScoresJson { get; private set; }

    public AiResponseId? AiResponseId { get; private set; }

    public DateTimeOffset ComputedAtUtc { get; private set; }

    public static Result<WeaknessProfile> Create(
        WeaknessProfileId id, Guid tenantId, Guid userId, string skillScoresJson, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(skillScoresJson))
        {
            return Result.Failure<WeaknessProfile>(WeaknessProfileDomainErrors.ScoresRequired);
        }

        return Result.Success(new WeaknessProfile(id, tenantId, userId, skillScoresJson, aiResponseId, nowUtc));
    }

    /// <summary>Overwrites this profile's scores with a freshly computed set (scheduled job or
    /// learner-triggered force refresh) — idempotent update, not a new row.</summary>
    public Result Recompute(string skillScoresJson, AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(skillScoresJson))
        {
            return Result.Failure(WeaknessProfileDomainErrors.ScoresRequired);
        }

        SkillScoresJson = skillScoresJson;
        AiResponseId = aiResponseId;
        ComputedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class WeaknessProfileDomainErrors
{
    public static readonly Error ScoresRequired = Error.Validation(
        "VALIDATION_WEAKNESS_PROFILE_SCORES_REQUIRED", "Weakness profile scores are required.");
}
