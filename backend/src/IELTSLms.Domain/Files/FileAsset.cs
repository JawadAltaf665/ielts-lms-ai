using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Files;

/// <summary>
/// Metadata for a stored media/document artifact (ERD ENT-057). The bytes live in object storage
/// (<c>IFileStorage</c>, Abstractions/Storage) under <see cref="StorageKey"/> — this row is only the
/// reference plus validated metadata; access is always through a signed, expiring URL, never this key
/// directly (Security Standards §10, API Design Standards §14.1). First exercised by Listening
/// (Phase 6 Sprint 5) for audio; Speaking (Sprint 7) reuses the identical pattern for learner-recorded
/// audio — get the pattern right here.
/// </summary>
public sealed class FileAsset : TenantAuditableEntity<FileAssetId>
{
    private FileAsset(
        FileAssetId id, Guid tenantId, Guid? ownerUserId, string storageKey, string fileName, string contentType,
        long sizeBytes, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        OwnerUserId = ownerUserId;
        StorageKey = storageKey;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        CreatedAtUtc = nowUtc;
    }

    private FileAsset()
    {
        StorageKey = null!;
        FileName = null!;
        ContentType = null!;
    }

    public Guid? OwnerUserId { get; private init; }

    /// <summary>Object-storage key. Internal only — never returned to a client; always resolved via a signed URL.</summary>
    public string StorageKey { get; private init; }

    public string FileName { get; private init; }

    public string ContentType { get; private init; }

    public long SizeBytes { get; private init; }

    /// <summary>
    /// Validates the type/size business rule (ERD ENT-057) for an already-uploaded object.
    /// <paramref name="allowedContentTypes"/>/<paramref name="maxSizeBytes"/> are supplied by the
    /// calling module (e.g. Listening's audio allowlist) rather than hardcoded here, since Files is a
    /// shared entity reused across modules with different rules. Malware scanning (ERD ENT-057) is out
    /// of this sprint's scope — no AV integration exists yet in this codebase.
    /// </summary>
    public static Result<FileAsset> Create(
        FileAssetId id, Guid tenantId, Guid? ownerUserId, string storageKey, string fileName, string contentType,
        long sizeBytes, IReadOnlyCollection<string> allowedContentTypes, long maxSizeBytes, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return Result.Failure<FileAsset>(FileDomainErrors.StorageKeyRequired);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<FileAsset>(FileDomainErrors.FileNameRequired);
        }

        if (!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<FileAsset>(FileDomainErrors.ContentTypeNotAllowed);
        }

        if (sizeBytes <= 0 || sizeBytes > maxSizeBytes)
        {
            return Result.Failure<FileAsset>(FileDomainErrors.SizeInvalid);
        }

        return Result.Success(new FileAsset(id, tenantId, ownerUserId, storageKey, fileName, contentType, sizeBytes, nowUtc));
    }
}

internal static class FileDomainErrors
{
    public static readonly Error StorageKeyRequired = Error.Validation(
        "VALIDATION_FILE_STORAGE_KEY_REQUIRED", "Storage key is required.");

    public static readonly Error FileNameRequired = Error.Validation(
        "VALIDATION_FILE_NAME_REQUIRED", "File name is required.");

    public static readonly Error ContentTypeNotAllowed = Error.Validation(
        "VALIDATION_FILE_CONTENT_TYPE_NOT_ALLOWED", "This file type is not allowed.");

    public static readonly Error SizeInvalid = Error.Validation(
        "VALIDATION_FILE_SIZE_INVALID", "File size exceeds the allowed maximum or is empty.");
}
