using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => RoleId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId);
        
        builder.Property(r => r.Name).HasMaxLength(256).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(1000);
        builder.Property(r => r.IsSystemRole).IsRequired();

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique()
            .HasDatabaseName("ix_roles_tenant_id_name");

        builder.Ignore(r => r.DomainEvents);
    }
}
