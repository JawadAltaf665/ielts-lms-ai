using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).ValueGeneratedNever();

        builder.Property(rp => rp.RoleId)
            .HasConversion(id => id.Value, value => RoleId.From(value))
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .HasConversion(id => id.Value, value => PermissionId.From(value))
            .IsRequired();

        builder.Property(rp => rp.TenantId).IsRequired();
        builder.Property(rp => rp.CreatedAtUtc).IsRequired();
        builder.Property(rp => rp.UpdatedAtUtc);
        builder.Property(rp => rp.CreatedByUserId).IsRequired();

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique()
            .HasDatabaseName("ix_role_permissions_role_permission");
    }
}
