using IELTSLms.Domain.Reading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Reading;

internal sealed class ReadingPassageConfiguration : IEntityTypeConfiguration<ReadingPassage>
{
    public void Configure(EntityTypeBuilder<ReadingPassage> builder)
    {
        builder.ToTable("reading_passages");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ReadingPassageId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog passage, visible read-only to every tenant.
        builder.Property(p => p.TenantId);
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PassageText).HasColumnType("text").IsRequired();
        builder.Property(p => p.Track).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(p => p.Difficulty).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(p => p.CategoryId);
        builder.Property(p => p.DurationSeconds).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_reading_passages_tenant_id");

        // Title uniqueness per catalog spec (ERD ENT-021): within a tenant's own passages, and
        // separately among global passages. Two partial indexes because Postgres treats every NULL
        // tenant_id as distinct, so a single (tenant_id, title) unique index would not constrain
        // global rows at all.
        builder.HasIndex(p => new { p.TenantId, p.Title })
            .IsUnique()
            .HasDatabaseName("ix_reading_passages_tenant_title_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(p => p.Title)
            .IsUnique()
            .HasDatabaseName("ix_reading_passages_global_title_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(p => p.DomainEvents);
    }
}
