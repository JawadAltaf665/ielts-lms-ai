using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Configuration;

public sealed class LoggingOptions
{
    public const string SectionName = "Logging:Behavior";

    [Range(100, 60000)]
    public int SlowRequestThresholdMs { get; set; } = 500;

    [MinLength(0)]
    public string[] MaskedProperties { get; set; } = ["password", "token", "secret", "authorization"];

    public bool EnableRequestBodyLogging { get; set; }
}
