using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("user_invitations");

        builder.HasKey(ui => ui.Id);
        builder.Property(ui => ui.Id)
            .HasConversion(id => id.Value, value => UserInvitationId.From(value))
            .ValueGeneratedNever();

        builder.Property(ui => ui.TenantId).IsRequired();

        builder.Property(ui => ui.Email)
            .HasConversion(email => email.Value, value => Email.FromTrusted(value))
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(ui => ui.RoleId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? RoleId.From(value.Value) : (RoleId?)null);

        builder.Property(ui => ui.Token).HasMaxLength(512).IsRequired();
        
        builder.Property(ui => ui.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        
        builder.Property(ui => ui.ExpiresAtUtc).IsRequired();
        builder.Property(ui => ui.CreatedAtUtc).IsRequired();
        builder.Property(ui => ui.UpdatedAtUtc);
        builder.Property(ui => ui.CreatedByUserId).IsRequired();

        builder.MapXminConcurrencyToken(ui => ui.Version);

        builder.HasIndex(ui => ui.Token)
            .IsUnique()
            .HasDatabaseName("ix_user_invitations_token");

        builder.Ignore(ui => ui.DomainEvents);
    }
}
