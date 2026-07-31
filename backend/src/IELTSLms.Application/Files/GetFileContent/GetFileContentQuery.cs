using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Files.GetFileContent;

/// <summary>
/// Resolves a signed streaming request to file bytes. Deliberately not <c>IAuthorizeRequest</c> — the
/// caller authenticates via the HMAC signature (<see cref="Signature"/>/<see cref="ExpiresAtUnixSeconds"/>),
/// the same trust model a cloud pre-signed URL uses, not a bearer token (API Design Standards §14.1:
/// "access via signed, expiring URLs").
/// </summary>
public sealed record GetFileContentQuery(string StorageKey, long ExpiresAtUnixSeconds, string Signature) : IQuery<FileContentResponse>;

public sealed record FileContentResponse(Stream Content, string ContentType, string FileName);
