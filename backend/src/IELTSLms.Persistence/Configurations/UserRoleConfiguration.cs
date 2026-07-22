using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id).ValueGeneratedNever();

        builder.Property(ur => ur.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasConversion(id => id.Value, value => RoleId.From(value))
            .IsRequired();

        builder.Property(ur => ur.TenantId).IsRequired();
        builder.Property(ur => ur.CreatedAtUtc).IsRequired();
        builder.Property(ur => ur.UpdatedAtUtc);
        builder.Property(ur => ur.CreatedByUserId).IsRequired();

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique()
            .HasDatabaseName("ix_user_roles_user_role");
    }
}
