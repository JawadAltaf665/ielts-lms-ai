using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Storage;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Files;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Files.GetFileContent;

public sealed class GetFileContentQueryHandler(IFileAssetRepository files, IFileStorage fileStorage, ITenantContextAccessor tenantContext)
    : IQueryHandler<GetFileContentQuery, FileContentResponse>
{
    private static readonly Error InvalidSignature = Error.Forbidden(
        "AUTHZ_FILE_SIGNATURE_INVALID", "This link is invalid or has expired.");

    private static readonly Error FileNotFound = Error.NotFound("NOT_FOUND_FILE", "File not found.");

    public async Task<Result<FileContentResponse>> Handle(GetFileContentQuery request, CancellationToken cancellationToken)
    {
        if (!fileStorage.ValidateSignedUrl(request.StorageKey, request.ExpiresAtUnixSeconds, request.Signature))
        {
            return Result.Failure<FileContentResponse>(InvalidSignature);
        }

        // The signature is the authorization for this request — there is no ambient tenant (the route
        // is unauthenticated), so the lookup bypasses the tenant filter deliberately.
        FileAsset? file;
        using (tenantContext.BypassTenantFilter())
        {
            file = await files.GetByStorageKeyAsync(request.StorageKey, cancellationToken).ConfigureAwait(false);
        }

        if (file is null)
        {
            return Result.Failure<FileContentResponse>(FileNotFound);
        }

        var content = await fileStorage.DownloadAsync(request.StorageKey, cancellationToken).ConfigureAwait(false);
        return Result.Success(new FileContentResponse(content, file.ContentType, file.FileName));
    }
}
