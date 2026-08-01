using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Reading;

/// <summary>
/// A reading passage plus its authored metadata (ERD ENT-021). <see cref="TenantId"/> is nullable:
/// <see langword="null"/> for a global-catalog passage authored once and visible to every tenant
/// (read-only to tenants), non-null for a tenant's own authored passage — same global-vs-tenant split
/// as <c>GrammarTopic</c> (Phase 6 Sprint 1). Because of the nullable tenant this does not implement
/// <see cref="ITenantScoped"/>, so it is exempt from the automatic tenant query filter; handlers must
/// filter explicitly (global OR caller's tenant).
/// </summary>
public sealed class ReadingPassage : AuditableEntity<ReadingPassageId>
{
    private ReadingPassage(
        ReadingPassageId id, Guid? tenantId, string title, string passageText, ReadingTrack track,
        ReadingDifficulty difficulty, Guid? categoryId, int durationSeconds, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        PassageText = passageText;
        Track = track;
        Difficulty = difficulty;
        CategoryId = categoryId;
        DurationSeconds = durationSeconds;
        CreatedAtUtc = nowUtc;
    }

    private ReadingPassage()
    {
        Title = null!;
        PassageText = null!;
    }

    /// <summary>Owning tenant, or <see langword="null"/> for a global-catalog passage.</summary>
    public Guid? TenantId { get; private init; }

    public string Title { get; private set; }

    public string PassageText { get; private set; }

    public ReadingTrack Track { get; private set; }

    public ReadingDifficulty Difficulty { get; private set; }

    public Guid? CategoryId { get; private set; }

    /// <summary>
    /// Server-configured attempt duration. Never client-supplied — <c>StartReadingAttempt</c> reads
    /// this to compute <see cref="ReadingAttempt.DeadlineAtUtc"/>, the caller cannot request a duration.
    /// </summary>
    public int DurationSeconds { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<ReadingPassage> Create(
        ReadingPassageId id, Guid? tenantId, string title, string passageText, ReadingTrack track,
        ReadingDifficulty difficulty, Guid? categoryId, int durationSeconds, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<ReadingPassage>(ReadingDomainErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(passageText))
        {
            return Result.Failure<ReadingPassage>(ReadingDomainErrors.PassageTextRequired);
        }

        if (durationSeconds <= 0)
        {
            return Result.Failure<ReadingPassage>(ReadingDomainErrors.DurationInvalid);
        }

        return Result.Success(
            new ReadingPassage(id, tenantId, title, passageText, track, difficulty, categoryId, durationSeconds, nowUtc));
    }

    /// <summary>Updates the passage's content. Global passages are read-only to tenants; callers must
    /// already have verified ownership before calling this (BUSINESS_READING_PASSAGE_READ_ONLY).</summary>
    public Result Update(
        string title, string passageText, ReadingTrack track, ReadingDifficulty difficulty, Guid? categoryId,
        int durationSeconds, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(ReadingDomainErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(passageText))
        {
            return Result.Failure(ReadingDomainErrors.PassageTextRequired);
        }

        if (durationSeconds <= 0)
        {
            return Result.Failure(ReadingDomainErrors.DurationInvalid);
        }

        Title = title;
        PassageText = passageText;
        Track = track;
        Difficulty = difficulty;
        CategoryId = categoryId;
        DurationSeconds = durationSeconds;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class ReadingDomainErrors
{
    public static readonly Error TitleRequired = Error.Validation(
        "VALIDATION_READING_TITLE_REQUIRED", "Title is required.");

    public static readonly Error PassageTextRequired = Error.Validation(
        "VALIDATION_READING_PASSAGE_TEXT_REQUIRED", "Passage text is required.");

    public static readonly Error DurationInvalid = Error.Validation(
        "VALIDATION_READING_DURATION_INVALID", "Duration must be greater than zero seconds.");
}
