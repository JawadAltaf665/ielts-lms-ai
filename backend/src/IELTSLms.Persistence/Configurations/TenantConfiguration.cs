using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="Tenant"/> aggregate (ERD ENT-001): strongly-typed id and value-object
/// conversions, globally-unique slug and code, the lifecycle <c>status</c>, branding/locale defaults,
/// audit and soft-delete columns, and the PostgreSQL <c>xmin</c> optimistic-concurrency token. The
/// tenant is the isolation root and is itself <em>not</em> tenant-scoped, so it carries no tenant
/// query filter — only the soft-delete filter.
/// </summary>
internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .HasColumnName("id")
            .ValueGeneratedNever();

        // Slug value object -> single normalized column; globally unique (host-based resolution).
        builder.Property(t => t.Slug)
            .HasConversion(slug => slug.Value, value => TenantSlug.FromTrusted(value))
            .HasColumnName("slug")
            .HasMaxLength(TenantSlug.MaxLength)
            .IsRequired();

        // Code value object -> single normalized column; globally unique human-facing reference.
        builder.Property(t => t.Code)
            .HasConversion(code => code.Value, value => TenantCode.FromTrusted(value))
            .HasColumnName("code")
            .HasMaxLength(TenantCode.MaxLength)
            .IsRequired();

        builder.Property(t => t.Name).HasMaxLength(Tenant.NameMaxLength).IsRequired();
        builder.Property(t => t.PrimaryDomain).HasMaxLength(Tenant.DomainMaxLength);
        builder.Property(t => t.ContactEmail).HasMaxLength(Tenant.ContactEmailMaxLength);
        builder.Property(t => t.DefaultLocale).HasMaxLength(Tenant.LocaleMaxLength).IsRequired();
        builder.Property(t => t.DefaultTimeZone).HasMaxLength(Tenant.LocaleMaxLength).IsRequired();

        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.DeletedAtUtc);

        // Map the domain's Version onto the PostgreSQL system column xmin as the concurrency token
        // (Backend Architecture §9), so optimistic concurrency needs no extra stored column.
        builder.MapXminConcurrencyToken(t => t.Version);

        // Slug and code are globally unique tenant identifiers (ERD ENT-001 unique constraints).
        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("ix_tenants_code");

        // Status is a common filter for administrative listings (ERD ENT-001 indexes).
        builder.HasIndex(t => t.Status).HasDatabaseName("ix_tenants_status");

        // Soft-deleted (closed) tenants are filtered out of normal queries (Development Standards §15).
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsActive);
    }
}
