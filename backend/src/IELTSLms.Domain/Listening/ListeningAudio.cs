using IELTSLms.Domain.Files;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Listening;

/// <summary>
/// An audio asset plus its authored metadata (ERD ENT-024). <see cref="TenantId"/> is nullable — the
/// same global-vs-tenant catalog split as <see cref="ReadingPassage"/> (Phase 6 Sprint 4):
/// <see langword="null"/> for a global-catalog track, non-null for a tenant's own authored track.
/// <see cref="FileId"/> points at the actual audio bytes via the shared <see cref="FileAsset"/>
/// (ENT-057) — this row never touches object storage directly. <see cref="TranscriptText"/> is
/// content-authored answer-key material (Sprint 5 scope note: not AI-generated, unlike Speaking's
/// future STT transcripts) and is only ever exposed in learning mode.
/// </summary>
public sealed class ListeningAudio : AuditableEntity<ListeningAudioId>
{
    private ListeningAudio(
        ListeningAudioId id, Guid? tenantId, string title, ReadingTrack track, FileAssetId fileId, Guid? categoryId,
        int durationSeconds, string? transcriptText, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Track = track;
        FileId = fileId;
        CategoryId = categoryId;
        DurationSeconds = durationSeconds;
        TranscriptText = transcriptText;
        CreatedAtUtc = nowUtc;
    }

    private ListeningAudio()
    {
        Title = null!;
    }

    public Guid? TenantId { get; private init; }

    public string Title { get; private set; }

    public ReadingTrack Track { get; private set; }

    /// <summary>The audio bytes, via the shared Files entity (ENT-057). Never <see cref="Guid.Empty"/>.</summary>
    public FileAssetId FileId { get; private init; }

    public Guid? CategoryId { get; private set; }

    /// <summary>Server-configured attempt duration — same "caller never supplies timing" rule as <see cref="ReadingPassage.DurationSeconds"/>.</summary>
    public int DurationSeconds { get; private set; }

    /// <summary>Content-authored transcript; exposed only in <see cref="ListeningMode.Learning"/>.</summary>
    public string? TranscriptText { get; private set; }

    public bool IsGlobal => TenantId is null;

    public static Result<ListeningAudio> Create(
        ListeningAudioId id, Guid? tenantId, string title, ReadingTrack track, FileAssetId fileId, Guid? categoryId,
        int durationSeconds, string? transcriptText, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<ListeningAudio>(ListeningDomainErrors.TitleRequired);
        }

        if (fileId.Value == Guid.Empty)
        {
            return Result.Failure<ListeningAudio>(ListeningDomainErrors.FileRequired);
        }

        if (durationSeconds <= 0)
        {
            return Result.Failure<ListeningAudio>(ListeningDomainErrors.DurationInvalid);
        }

        return Result.Success(new ListeningAudio(id, tenantId, title, track, fileId, categoryId, durationSeconds, transcriptText, nowUtc));
    }

    /// <summary>Updates the audio's metadata. Global audios are read-only to tenants; callers must already have verified ownership.</summary>
    public Result Update(
        string title, ReadingTrack track, Guid? categoryId, int durationSeconds, string? transcriptText, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(ListeningDomainErrors.TitleRequired);
        }

        if (durationSeconds <= 0)
        {
            return Result.Failure(ListeningDomainErrors.DurationInvalid);
        }

        Title = title;
        Track = track;
        CategoryId = categoryId;
        DurationSeconds = durationSeconds;
        TranscriptText = transcriptText;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class ListeningDomainErrors
{
    public static readonly Error TitleRequired = Error.Validation(
        "VALIDATION_LISTENING_TITLE_REQUIRED", "Title is required.");

    public static readonly Error FileRequired = Error.Validation(
        "VALIDATION_LISTENING_FILE_REQUIRED", "An audio file is required.");

    public static readonly Error DurationInvalid = Error.Validation(
        "VALIDATION_LISTENING_DURATION_INVALID", "Duration must be greater than zero seconds.");
}
