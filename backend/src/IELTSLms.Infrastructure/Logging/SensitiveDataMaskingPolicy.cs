using Serilog.Core;
using Serilog.Events;

namespace IELTSLms.Infrastructure.Logging;

/// <summary>
/// Serilog destructuring policy that replaces sensitive property values with "***REDACTED***".
/// </summary>
public sealed class SensitiveDataMaskingPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordHash", "token", "secret", "authorization",
        "refreshToken", "accessToken", "apiKey", "connectionString",
    };

    public bool TryDestructure(
        object value,
        ILogEventPropertyValueFactory propertyValueFactory,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        result = null;
        return false;
    }

    public static bool IsSensitive(string propertyName) =>
        SensitiveKeys.Any(k => propertyName.Contains(k, StringComparison.OrdinalIgnoreCase));
}
