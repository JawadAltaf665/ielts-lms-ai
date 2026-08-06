using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Speaking;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.WeaknessDetection;

/// <summary>
/// Progress Analysis Agent step (Sprint 9, Agentic AI Architecture §12.2/§13.3): gathers cross-skill
/// history (read-only — same "most recent rows per skill" shape as <c>BandPredictionJob</c>), enforces
/// the minimum-data-threshold gate (business rule: below threshold, return "insufficient data yet"
/// rather than a low-confidence guess — mirrors FR-BAND-001), then calls the AI Gateway to compute
/// per-skill weakness scores and upserts the learner's <see cref="WeaknessProfile"/>. Shared by
/// <c>ProgressAnalysisJob</c> (learner-triggered force refresh, AiOperation-tracked) and
/// <c>DailyLearningWorkflowRunner</c>'s step 1 (scheduled/lazy-fallback, untracked) — both need the
/// identical gate + computation, only the surrounding bookkeeping differs.
/// ponytail: duplicates BandPredictionJob's per-skill "gather most recent rows" shape rather than a
/// shared cross-skill read model/materialized view — acceptable while per-learner history stays small,
/// same call as that job made; revisit both together if this needs to change.
/// </summary>
public sealed class ProgressAnalysisService(
    IProgressTrackingRepository progressTracking, IWeaknessProfileRepository weaknessProfiles,
    IAiProviderRepository providers, IAiPromptRepository prompts, IAiResponseRepository responses,
    IWritingSubmissionRepository writingSubmissions, IWritingEvaluationRepository writingEvaluations,
    ISpeakingAttemptRepository speakingAttempts, ISpeakingEvaluationRepository speakingEvaluations,
    IReadingAttemptRepository readingAttempts, IListeningAttemptRepository listeningAttempts,
    IGrammarAttemptRepository grammarAttempts, IVocabularyProgressRepository vocabularyProgress,
    IAiGateway gateway, IGuidProvider guids)
{
    /// <summary>Minimum total activity count (summed across the six relevant skills) before a profile
    /// is computed at all (business rule).</summary>
    public const int MinTotalActivities = 5;

    /// <summary>Minimum number of distinct skills with at least one recorded activity.</summary>
    public const int MinSkillsWithActivity = 2;

    private const int MaxOutputTokens = 768;
    private const int MaxItemsPerSkill = 5;

    private static readonly LearningSkill[] RelevantSkills =
    [
        LearningSkill.Grammar, LearningSkill.Vocabulary, LearningSkill.Reading,
        LearningSkill.Listening, LearningSkill.Writing, LearningSkill.Speaking,
    ];

    public async Task<bool> HasSufficientDataAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var rows = await progressTracking.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        return IsSufficient(rows);
    }

    /// <summary>The minimum-data-threshold gate, isolated as a pure function so it can be unit-tested
    /// without any AI/repository dependencies (Testing §15 requirement).</summary>
    internal static bool IsSufficient(IReadOnlyList<ProgressTracking> rows)
    {
        var relevant = rows.Where(r => RelevantSkills.Contains(r.Skill)).ToList();
        var total = relevant.Sum(r => r.ActivityCount);
        var skillsWithActivity = relevant.Count(r => r.ActivityCount > 0);
        return total >= MinTotalActivities && skillsWithActivity >= MinSkillsWithActivity;
    }

    /// <summary>
    /// Computes and upserts the learner's <see cref="WeaknessProfile"/>. Returns a successful
    /// <see langword="null"/> when the minimum-data threshold isn't met — that is not a failure, the
    /// caller must not burn AI budget or surface it as an error. Adds/mutates the profile via the
    /// repository but does not call <c>SaveChangesAsync</c> — the caller owns the transaction boundary
    /// (same convention as the other AI capability jobs).
    /// </summary>
    public async Task<Result<WeaknessProfile?>> ComputeAsync(
        Guid tenantId, Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var progress = await progressTracking.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        if (!IsSufficient(progress))
        {
            return Result.Success<WeaknessProfile?>(null);
        }

        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.ProgressAnalysis, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            return Result.Failure<WeaknessProfile?>(AiErrors.PromptNotConfigured);
        }

        var existing = await weaknessProfiles.GetByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        var profileId = existing?.Id ?? WeaknessProfileId.From(guids.NewGuid());

        var history = await GatherHistoryAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        var inputJson = JsonSerializer.Serialize(new ProgressAnalysisInput(history), LearningJsonOptions.Instance);

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.ProgressAnalysis, prompt.PromptVersion, provider.Kind, prompt.SystemTemplate,
                inputJson, ProgressAnalysisSchema.Name, ProgressAnalysisSchema.Json, MaxOutputTokens),
            cancellationToken).ConfigureAwait(false);

        if (invocation.IsFailure)
        {
            return Result.Failure<WeaknessProfile?>(invocation.Error);
        }

        var invocationResult = invocation.Value;
        var output = TryDeserialize<ProgressAnalysisOutput>(invocationResult.OutputJson);

        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(WeaknessProfile), profileId.Value, ComputeHash(inputJson), invocationResult.Model,
            invocationResult.PromptTokens, invocationResult.CompletionTokens, invocationResult.LatencyMs,
            invocationResult.CostMinorUnits, invocationResult.CostCurrency,
            output is not null ? AiResponseStatus.Succeeded : AiResponseStatus.Invalid,
            output is not null ? invocationResult.OutputJson : null, invocationResult.ProviderResponseId, now);

        if (responseCreate.IsFailure)
        {
            return Result.Failure<WeaknessProfile?>(AiErrors.InputRejected);
        }

        var response = responseCreate.Value;
        responses.Add(response);

        if (output is null)
        {
            return Result.Failure<WeaknessProfile?>(AiErrors.InputRejected);
        }

        var scores = output.Scores
            .Select(s => (Parsed: Enum.TryParse<LearningSkill>(s.Skill, out var skill), Skill: skill, s.WeaknessScore))
            .Where(s => s.Parsed)
            .Select(s => new SkillWeaknessScoreDto(s.Skill, s.WeaknessScore, history.Count(h => h.Skill == s.Skill.ToString())))
            .ToList();
        var scoresJson = JsonSerializer.Serialize(scores, LearningJsonOptions.Instance);

        if (existing is not null)
        {
            var recompute = existing.Recompute(scoresJson, response.Id, now);
            return recompute.IsFailure
                ? Result.Failure<WeaknessProfile?>(recompute.Error)
                : Result.Success<WeaknessProfile?>(existing);
        }

        var created = WeaknessProfile.Create(profileId, tenantId, userId, scoresJson, response.Id, now);
        if (created.IsFailure)
        {
            return Result.Failure<WeaknessProfile?>(created.Error);
        }

        weaknessProfiles.Add(created.Value);
        return Result.Success<WeaknessProfile?>(created.Value);
    }

    private async Task<List<ProgressHistoryItem>> GatherHistoryAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var history = new List<ProgressHistoryItem>();

        var submissions = await writingSubmissions.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        foreach (var submission in submissions.Take(MaxItemsPerSkill))
        {
            decimal? band = null;
            if (submission.Status == WritingSubmissionStatus.Evaluated && submission.WritingEvaluationId is { } evaluationId)
            {
                var evaluation = await writingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false);
                band = evaluation?.EstimatedBand;
            }

            history.Add(new ProgressHistoryItem("Writing", submission.CreatedAtUtc, band, null, null));
        }

        var speaking = await speakingAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        foreach (var attempt in speaking.Take(MaxItemsPerSkill))
        {
            decimal? band = null;
            if (attempt.Status == SpeakingAttemptStatus.Evaluated && attempt.SpeakingEvaluationId is { } evaluationId)
            {
                var evaluation = await speakingEvaluations.GetByIdAsync(evaluationId, cancellationToken).ConfigureAwait(false);
                band = evaluation?.EstimatedBand;
            }

            history.Add(new ProgressHistoryItem("Speaking", attempt.CreatedAtUtc, band, null, null));
        }

        var reading = await readingAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(reading.Take(MaxItemsPerSkill).Select(a => new ProgressHistoryItem("Reading", a.CreatedAtUtc, a.Band, a.RawScore, a.MaxScore)));

        var listening = await listeningAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(listening.Take(MaxItemsPerSkill).Select(a => new ProgressHistoryItem("Listening", a.CreatedAtUtc, a.Band, a.RawScore, a.MaxScore)));

        var grammar = await grammarAttempts.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(grammar.Take(MaxItemsPerSkill).Select(a => new ProgressHistoryItem("Grammar", a.CreatedAtUtc, null, a.RawScore, a.MaxScore)));

        var vocabulary = await vocabularyProgress.ListByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        history.AddRange(
            vocabulary.Where(v => v.LastGrade is not null).OrderByDescending(v => v.UpdatedAtUtc).Take(MaxItemsPerSkill)
                .Select(v => new ProgressHistoryItem("Vocabulary", v.UpdatedAtUtc ?? v.CreatedAtUtc, null, v.LastGrade, 5)));

        return history;
    }

    private static T? TryDeserialize<T>(string? json) where T : class
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, LearningJsonOptions.Instance);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private sealed record ProgressHistoryItem(string Skill, DateTimeOffset AtUtc, decimal? Band, int? RawScore, int? MaxScore);

    private sealed record ProgressAnalysisInput(IReadOnlyList<ProgressHistoryItem> RecentActivity);
}
