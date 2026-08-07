using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.WeaknessDetection.Queries.GetWeaknessProfile;

public sealed class GetWeaknessProfileQueryHandler(
    IWeaknessProfileRepository weaknessProfiles, IProgressTrackingRepository progressTracking)
    : IQueryHandler<GetWeaknessProfileQuery, WeaknessProfileResponse>
{
    public async Task<Result<WeaknessProfileResponse>> Handle(GetWeaknessProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await weaknessProfiles.GetByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        if (profile is not null)
        {
            var scores = JsonSerializer.Deserialize<List<SkillWeaknessScoreDto>>(profile.SkillScoresJson, LearningJsonOptions.Instance) ?? [];
            return Result.Success(new WeaknessProfileResponse(profile.Id.Value, "Ready", scores, profile.ComputedAtUtc));
        }

        var rows = await progressTracking.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        var status = ProgressAnalysisService.IsSufficient(rows) ? "NotComputedYet" : "InsufficientData";
        return Result.Success(new WeaknessProfileResponse(null, status, null, null));
    }
}
