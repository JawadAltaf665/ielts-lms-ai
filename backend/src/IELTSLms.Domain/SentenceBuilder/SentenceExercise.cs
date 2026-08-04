using System.Text.Json;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.SentenceBuilder;

/// <summary>
/// A sentence-construction drill (ERD ENT-019, cataloged but unbuilt until this sprint — see Sprint 7's
/// gap note). <see cref="TenantId"/> is nullable: <see langword="null"/> for global-catalog exercises
/// authored once and visible to every tenant (read-only to tenants), non-null for a tenant's own
/// authored exercise — mirrors <see cref="Grammar.GrammarTopic"/>'s system-vs-custom split. Because of
/// the nullable tenant this does not implement <see cref="ITenantScoped"/>, so it is exempt from the
/// automatic tenant query filter; handlers must filter explicitly (global OR caller's tenant).
/// </summary>
public sealed class SentenceExercise : AuditableEntity<SentenceExerciseId>
{
    private SentenceExercise(
        SentenceExerciseId id, Guid? tenantId, string prompt, string targetPatternJson, SentenceDifficulty difficulty,
        DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Prompt = prompt;
        TargetPatternJson = targetPatternJson;
        Difficulty = difficulty;
        CreatedAtUtc = nowUtc;
    }

    private SentenceExercise()
    {
        Prompt = null!;
        TargetPatternJson = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog exercise.</summary>
    public Guid? TenantId { get; private init; }

    public string Prompt { get; private set; }

    /// <summary>JSON array of required tokens the target grammatical pattern is built from — see
    /// <see cref="SentencePatternMatcher"/>.</summary>
    public string TargetPatternJson { get; private set; }

    public SentenceDifficulty Difficulty { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<SentenceExercise> Create(
        SentenceExerciseId id, Guid? tenantId, string prompt, string targetPatternJson, SentenceDifficulty difficulty,
        DateTimeOffset nowUtc)
    {
        var validation = Validate(prompt, targetPatternJson);
        if (validation is not null)
        {
            return Result.Failure<SentenceExercise>(validation);
        }

        return Result.Success(new SentenceExercise(id, tenantId, prompt, targetPatternJson, difficulty, nowUtc));
    }

    public void SoftDelete(DateTimeOffset nowUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    private static Error? Validate(string prompt, string targetPatternJson)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return SentenceBuilderDomainErrors.PromptRequired;
        }

        if (!HasAtLeastOneToken(targetPatternJson))
        {
            return SentenceBuilderDomainErrors.TargetPatternRequired;
        }

        return null;
    }

    private static bool HasAtLeastOneToken(string targetPatternJson)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(targetPatternJson) is { Length: > 0 };
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

internal static class SentenceBuilderDomainErrors
{
    public static readonly Error PromptRequired = Error.Validation(
        "VALIDATION_SENTENCE_EXERCISE_PROMPT_REQUIRED", "Prompt is required.");

    public static readonly Error TargetPatternRequired = Error.Validation(
        "VALIDATION_SENTENCE_EXERCISE_TARGET_PATTERN_REQUIRED", "Target pattern must be a non-empty JSON array of tokens.");
}
