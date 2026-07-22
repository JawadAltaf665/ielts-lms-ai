namespace IELTSLms.Domain.Identity;

/// <summary>
/// Strongly-typed identifier for a <see cref="UserInvitation"/>.
/// </summary>
public readonly record struct UserInvitationId(Guid Value)
{
    public static UserInvitationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
