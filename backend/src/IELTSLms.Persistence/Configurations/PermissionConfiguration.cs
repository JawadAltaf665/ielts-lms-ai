using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PermissionId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.Key).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.Group).HasMaxLength(100);
        builder.Property(p => p.Metadata).HasColumnType("jsonb");
        builder.Property(p => p.PermissionVersion).HasMaxLength(50);
        
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.Key)
            .IsUnique()
            .HasDatabaseName("ix_permissions_key");

        builder.Ignore(p => p.DomainEvents);
    }
}
