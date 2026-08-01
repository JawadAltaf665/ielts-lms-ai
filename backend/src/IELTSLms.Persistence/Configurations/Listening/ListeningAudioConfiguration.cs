using IELTSLms.Domain.Files;
using IELTSLms.Domain.Listening;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Listening;

internal sealed class ListeningAudioConfiguration : IEntityTypeConfiguration<ListeningAudio>
{
    public void Configure(EntityTypeBuilder<ListeningAudio> builder)
    {
        builder.ToTable("listening_audios");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => ListeningAudioId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog audio, visible read-only to every tenant.
        builder.Property(a => a.TenantId);
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Track).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(a => a.FileId)
            .HasConversion(id => id.Value, value => FileAssetId.From(value))
            .IsRequired();

        builder.Property(a => a.CategoryId);
        builder.Property(a => a.DurationSeconds).IsRequired();
        builder.Property(a => a.TranscriptText).HasColumnType("text");

        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);

        builder.HasIndex(a => a.TenantId).HasDatabaseName("ix_listening_audios_tenant_id");
        builder.HasIndex(a => a.Track).HasDatabaseName("ix_listening_audios_track");

        // Title uniqueness per catalog spec (ERD ENT-024): within a tenant's own audios, and separately
        // among global audios — two partial indexes, same reasoning as ReadingPassageConfiguration.
        builder.HasIndex(a => new { a.TenantId, a.Title })
            .IsUnique()
            .HasDatabaseName("ix_listening_audios_tenant_title_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(a => a.Title)
            .IsUnique()
            .HasDatabaseName("ix_listening_audios_global_title_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(a => a.DomainEvents);
    }
}
