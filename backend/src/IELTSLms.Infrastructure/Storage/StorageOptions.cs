namespace IELTSLms.Infrastructure.Storage;

/// <summary>
/// Binds the <c>Storage</c> configuration section. Deliberately not <c>ValidateOnStart()</c> — same
/// reasoning as <see cref="Billing.StripeOptions"/>/<see cref="AI.Claude.ClaudeOptions"/>: a blank
/// signing key must not prevent the whole API from starting in local dev or CI; absence is caught at
/// first sign/validate call instead (<see cref="LocalFileStorage"/>).
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Filesystem root files are written under. Relative paths resolve against the content root.</summary>
    public string RootPath { get; set; } = "App_Data/storage";

    /// <summary>HMAC secret for signing/validating streaming URLs. Never committed — set per environment.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Route prefix the signed URL points at; must match <c>FilesEndpoints</c>' mapped route.</summary>
    public string StreamPathPrefix { get; set; } = "/api/v1/files/stream";
}
