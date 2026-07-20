using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Abstractions.Audit;

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public bool EnableAuditLog { get; set; } = true;
    public bool EnableActivityLog { get; set; } = true;

    [MinLength(0)]
    public string[] ExcludedEntities { get; set; } = [];

    [MinLength(0)]
    public string[] ExcludedProperties { get; set; } = ["PasswordHash", "SecurityStamp"];
}
