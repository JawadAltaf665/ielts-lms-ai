using System;
using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Expired = 3
}

/// <summary>
/// Tracks the lifecycle of a user invitation to a tenant.
/// </summary>
public sealed class UserInvitation : AggregateRoot<UserInvitationId>, IAuditableEntity, ITenantScoped
{
    private UserInvitation(UserInvitationId id, Guid tenantId, Email email, RoleId? roleId, string token, DateTimeOffset expiresAtUtc, DateTimeOffset createdAtUtc, Guid createdByUserId)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        RoleId = roleId;
        Token = token;
        Status = InvitationStatus.Pending;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    private UserInvitation()
    {
        Email = null!;
        Token = null!;
    }

    public Guid TenantId { get; private init; }
    public Email Email { get; private init; }
    public RoleId? RoleId { get; private init; }
    public string Token { get; private init; }
    public InvitationStatus Status { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private init; }
    public uint Version { get; private set; }

    public static UserInvitation Invite(UserInvitationId id, Guid tenantId, Email email, RoleId? roleId, string token, DateTimeOffset expiresAtUtc, DateTimeOffset nowUtc, Guid createdByUserId)
    {
        return new UserInvitation(id, tenantId, email, roleId, token, expiresAtUtc, nowUtc, createdByUserId);
    }

    public void Accept(DateTimeOffset nowUtc)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invitations can be accepted.");
        }
        if (nowUtc > ExpiresAtUtc)
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = InvitationStatus.Accepted;
        UpdatedAtUtc = nowUtc;
    }

    public void Reject(DateTimeOffset nowUtc)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invitations can be rejected.");
        }
        
        Status = InvitationStatus.Rejected;
        UpdatedAtUtc = nowUtc;
    }

    public void Expire(DateTimeOffset nowUtc)
    {
        if (Status != InvitationStatus.Pending)
        {
            return;
        }

        Status = InvitationStatus.Expired;
        UpdatedAtUtc = nowUtc;
    }
}
