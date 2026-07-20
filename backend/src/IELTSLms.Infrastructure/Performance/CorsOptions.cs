using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Performance;

/// <summary>
/// Binds the <c>Cors</c> configuration section. Production origins are explicitly listed;
/// wildcard is only allowed in development (Security Standards §12.5).
/// </summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Allowed origins for CORS. Empty or ["*"] in dev allows any origin.</summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>Allowed HTTP methods.</summary>
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    /// <summary>Allowed request headers.</summary>
    public string[] AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Correlation-Id", "X-Tenant-Id"];

    /// <summary>Headers exposed to the client.</summary>
    public string[] ExposedHeaders { get; set; } = ["X-Correlation-Id", "X-Request-Id", "Retry-After"];

    /// <summary>Preflight max age in seconds.</summary>
    [Range(0, 86400)]
    public int MaxAgeSeconds { get; set; } = 600;

    /// <summary>Whether credentials (cookies, auth headers) are allowed.</summary>
    public bool AllowCredentials { get; set; } = true;
}
