using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A registered or trusted device for a user.
/// </summary>
public sealed class KnownDevice : AggregateRoot<KnownDeviceId>, IAuditableEntity, ITenantScoped
{
    private KnownDevice(KnownDeviceId id, Guid tenantId, UserId userId, string deviceIdentifier, string deviceName, bool isTrusted, DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DeviceIdentifier = deviceIdentifier;
        DeviceName = deviceName;
        IsTrusted = isTrusted;
        LastSeenAtUtc = createdAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    private KnownDevice()
    {
        DeviceIdentifier = null!;
        DeviceName = null!;
    }

    public Guid TenantId { get; private init; }
    public UserId UserId { get; private init; }
    
    /// <summary>A hash or unique fingerprint of the device.</summary>
    public string DeviceIdentifier { get; private init; }
    
    /// <summary>User-friendly name (e.g. 'Jawad's iPhone').</summary>
    public string DeviceName { get; private set; }
    public bool IsTrusted { get; private set; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }
    
    public DateTimeOffset? RemovedAtUtc { get; private set; }
    public bool IsRemoved => RemovedAtUtc.HasValue;

    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public uint Version { get; private set; }

    public static KnownDevice Register(KnownDeviceId id, Guid tenantId, UserId userId, string deviceIdentifier, string deviceName, bool isTrusted, DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceIdentifier);
        return new KnownDevice(id, tenantId, userId, deviceIdentifier, deviceName, isTrusted, nowUtc);
    }

    public void MarkSeen(DateTimeOffset nowUtc)
    {
        if (IsRemoved)
        {
            return;
        }
        LastSeenAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Trust(DateTimeOffset nowUtc)
    {
        if (IsRemoved)
        {
            return;
        }
        if (IsTrusted)
        {
            return;
        }
        
        IsTrusted = true;
        UpdatedAtUtc = nowUtc;
    }

    public void Revoke(DateTimeOffset nowUtc)
    {
        if (IsRemoved)
        {
            return;
        }

        IsTrusted = false;
        RemovedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
