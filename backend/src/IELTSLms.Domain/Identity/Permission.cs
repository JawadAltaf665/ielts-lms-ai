using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

public enum PermissionStatus
{
    Active = 0,
    Deprecated = 1
}

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
/// <summary>
/// Fine-grained authorization primitive (ENT-006). Global reference data.
/// </summary>
public sealed class Permission : AggregateRoot<PermissionId>, IAuditableEntity
{
    private Permission(PermissionId id, string key, string name, string description, string category, string group, string metadata, string version, DateTimeOffset createdAtUtc)
        : base(id)
    {
        Key = key;
        Name = name;
        Description = description;
        Category = category;
        Group = group;
        Metadata = metadata;
        PermissionVersion = version;
        Status = PermissionStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    private Permission()
    {
        Key = null!;
        Name = null!;
        Description = null!;
        Category = null!;
        Group = null!;
        Metadata = null!;
        PermissionVersion = null!;
    }

    /// <summary>The stable capability key, e.g. "writing.evaluate".</summary>
    public string Key { get; private init; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public string Group { get; private set; }
    
    /// <summary>JSON string containing additional capability rules if needed.</summary>
    public string Metadata { get; private set; }
    public string PermissionVersion { get; private set; }
    public PermissionStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public uint Version { get; private set; }

    public static Permission Create(PermissionId id, string key, string name, string description, string category, string group, string metadata, string version, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new Permission(id, key, name, description, category, group, metadata, version, nowUtc);
    }

    public void Deprecate(DateTimeOffset nowUtc)
    {
        if (Status == PermissionStatus.Deprecated)
        {
            return;
        }
        Status = PermissionStatus.Deprecated;
        UpdatedAtUtc = nowUtc;
    }
}
