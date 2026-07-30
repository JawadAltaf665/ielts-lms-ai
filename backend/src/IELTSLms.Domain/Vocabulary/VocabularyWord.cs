using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Vocabulary;

/// <summary>
/// A vocabulary item for study (ERD ENT-017, DDD §5 BC-08). <see cref="TenantId"/> is nullable:
/// <see langword="null"/> for global-catalog words authored once and visible to every tenant
/// (read-only to tenants), non-null for a tenant's own authored word — mirrors
/// <see cref="Grammar.GrammarTopic"/>'s system-vs-custom split. Because of the nullable tenant this
/// does not implement <see cref="ITenantScoped"/>, so it is exempt from the automatic tenant query
/// filter; handlers must filter explicitly (global OR caller's tenant).
/// </summary>
public sealed class VocabularyWord : AuditableEntity<VocabularyWordId>
{
    private VocabularyWord(
        VocabularyWordId id, Guid? tenantId, string word, string sense, string meaning, string examplesJson,
        VocabularyFrequencyBand frequencyBand, string? topic, Guid? categoryId, VocabularyDifficultyLevel difficultyLevel,
        DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Word = word;
        Sense = sense;
        Meaning = meaning;
        ExamplesJson = examplesJson;
        FrequencyBand = frequencyBand;
        Topic = topic;
        CategoryId = categoryId;
        DifficultyLevel = difficultyLevel;
        CreatedAtUtc = nowUtc;
    }

    private VocabularyWord()
    {
        Word = null!;
        Sense = null!;
        Meaning = null!;
        ExamplesJson = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog word.</summary>
    public Guid? TenantId { get; private init; }

    public string Word { get; private init; }

    /// <summary>Disambiguates one meaning of <see cref="Word"/> from another (e.g. "financial institution" vs "riverbank").</summary>
    public string Sense { get; private set; }

    public string Meaning { get; private set; }

    /// <summary>JSON array of example sentences.</summary>
    public string ExamplesJson { get; private set; }

    public VocabularyFrequencyBand FrequencyBand { get; private set; }

    public string? Topic { get; private set; }

    public Guid? CategoryId { get; private set; }

    public VocabularyDifficultyLevel DifficultyLevel { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<VocabularyWord> Create(
        VocabularyWordId id, Guid? tenantId, string word, string sense, string meaning, string examplesJson,
        VocabularyFrequencyBand frequencyBand, string? topic, Guid? categoryId, VocabularyDifficultyLevel difficultyLevel,
        DateTimeOffset nowUtc)
    {
        var validation = Validate(word, sense, meaning);
        if (validation is not null)
        {
            return Result.Failure<VocabularyWord>(validation);
        }

        return Result.Success(new VocabularyWord(
            id, tenantId, word, sense, meaning, examplesJson, frequencyBand, topic, categoryId, difficultyLevel, nowUtc));
    }

    /// <summary>Updates the word's content. <see cref="Word"/> itself is immutable — a changed headword is a
    /// different word. Global words are read-only to tenants; callers must already have verified ownership
    /// before calling this (BUSINESS_VOCABULARY_WORD_READ_ONLY).</summary>
    public Result Update(
        string sense, string meaning, string examplesJson, VocabularyFrequencyBand frequencyBand, string? topic,
        Guid? categoryId, VocabularyDifficultyLevel difficultyLevel, DateTimeOffset nowUtc)
    {
        var validation = Validate(Word, sense, meaning);
        if (validation is not null)
        {
            return Result.Failure(validation);
        }

        Sense = sense;
        Meaning = meaning;
        ExamplesJson = examplesJson;
        FrequencyBand = frequencyBand;
        Topic = topic;
        CategoryId = categoryId;
        DifficultyLevel = difficultyLevel;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
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

    private static Error? Validate(string word, string sense, string meaning)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return VocabularyDomainErrors.WordRequired;
        }

        if (string.IsNullOrWhiteSpace(sense))
        {
            return VocabularyDomainErrors.SenseRequired;
        }

        if (string.IsNullOrWhiteSpace(meaning))
        {
            return VocabularyDomainErrors.MeaningRequired;
        }

        return null;
    }
}

internal static class VocabularyDomainErrors
{
    public static readonly Error WordRequired = Error.Validation(
        "VALIDATION_VOCABULARY_WORD_REQUIRED", "Word is required.");

    public static readonly Error SenseRequired = Error.Validation(
        "VALIDATION_VOCABULARY_SENSE_REQUIRED", "Sense is required.");

    public static readonly Error MeaningRequired = Error.Validation(
        "VALIDATION_VOCABULARY_MEANING_REQUIRED", "Meaning is required.");
}
