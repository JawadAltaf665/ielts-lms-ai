using IELTSLms.Api.Extensions;
using IELTSLms.Application.Files.GetFileContent;
using MediatR;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Serves file bytes for a signed, expiring URL previously issued by <c>IFileStorage.GetSignedUrlAsync</c>
/// (Sprint 5, API Design Standards §14.1: "direct execution is never permitted; access via signed,
/// expiring URLs"). This is the only reachable route to an uploaded file's bytes — there is no
/// authenticated-but-unsigned download path. Anonymous like <see cref="WebhookEndpoints"/>: the
/// signature is the authorization, not a bearer token.
/// </summary>
internal static class FilesEndpoints
{
    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/stream", StreamFileAsync)
            .AllowAnonymous()
            .WithTags("Files")
            .WithName("StreamFile")
            .WithSummary("Streams file bytes for a signed, expiring URL (signature-verified, not JWT-authenticated).");

        return builder;
    }

    private static async Task<IResult> StreamFileAsync(
        string key, long expires, string sig, ISender sender, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFileContentQuery(key, expires, sig), cancellationToken);
        return result.IsSuccess
            ? Results.Stream(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : result.ToProblem(httpContext);
    }
}
