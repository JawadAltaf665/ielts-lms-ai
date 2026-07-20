using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Base EF Core configuration for tenant-scoped, auditable entities. Concrete configurations inherit
/// this and call <see cref="ConfigureBase"/> to map the standard audit, soft-delete, tenant, and
/// concurrency columns consistently. Entity-specific columns are mapped in <see cref="ConfigureEntity"/>.
/// </summary>
internal abstract class BaseEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity<TId>
    where TId : notnull
{
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName);
        ConfigureBase(builder);
        ConfigureEntity(builder);
    }

    protected virtual void ConfigureBase(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        // Audit timestamps
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");

        // Soft delete
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(e => e.DeletedBy).HasColumnName("deleted_by");

        // Restore
        builder.Property(e => e.RestoredAtUtc).HasColumnName("restored_at_utc");
        builder.Property(e => e.RestoredBy).HasColumnName("restored_by");

        // Concurrency (xmin)
        builder.Property(e => e.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Tenant scope (only if the entity implements ITenantScoped)
        if (typeof(ITenantScoped).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property(nameof(ITenantScoped.TenantId))
                .HasColumnName("tenant_id")
                .IsRequired();

            builder.HasIndex(nameof(ITenantScoped.TenantId))
                .HasDatabaseName($"ix_{TableName}_tenant_id");
        }
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
