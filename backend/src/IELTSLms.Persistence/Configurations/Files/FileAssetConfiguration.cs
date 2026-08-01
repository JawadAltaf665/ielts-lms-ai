using IELTSLms.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Files;

internal sealed class FileAssetConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("files");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FileAssetId.From(value))
            .ValueGeneratedNever();

        builder.Property(f => f.OwnerUserId);
        builder.Property(f => f.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(f => f.FileName).HasMaxLength(255).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(f => f.SizeBytes).IsRequired();

        builder.Property(f => f.CreatedAtUtc).IsRequired();
        builder.Property(f => f.UpdatedAtUtc);
        builder.Property(f => f.IsDeleted).IsRequired();
        builder.Property(f => f.DeletedAtUtc);

        builder.MapXminConcurrencyToken(f => f.Version);
        builder.MapTenantScope("files");

        builder.HasIndex(f => new { f.TenantId, f.OwnerUserId }).HasDatabaseName("ix_files_tenant_owner");

        // Storage key uniqueness is global (ERD ENT-057: "Unique Constraints: Storage key (global)").
        builder.HasIndex(f => f.StorageKey).IsUnique().HasDatabaseName("ix_files_storage_key_unique");

        builder.Ignore(f => f.DomainEvents);
    }
}
