using IELTSLms.Domain.Speaking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Speaking;

internal sealed class SpeakingTaskConfiguration : IEntityTypeConfiguration<SpeakingTask>
{
    public void Configure(EntityTypeBuilder<SpeakingTask> builder)
    {
        builder.ToTable("speaking_tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => SpeakingTaskId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog task, visible read-only to every tenant.
        builder.Property(t => t.TenantId);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Part).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(t => t.PromptText).HasColumnType("text").IsRequired();
        builder.Property(t => t.CategoryId);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.DeletedAtUtc);

        builder.MapXminConcurrencyToken(t => t.Version);

        builder.HasIndex(t => t.TenantId).HasDatabaseName("ix_speaking_tasks_tenant_id");
        builder.HasIndex(t => t.Part).HasDatabaseName("ix_speaking_tasks_part");

        // Two partial indexes, same reasoning as WritingTask/ListeningAudio: Postgres treats every NULL
        // tenant_id as distinct, so a single (tenant_id, title) unique index would not constrain global rows.
        builder.HasIndex(t => new { t.TenantId, t.Title })
            .IsUnique()
            .HasDatabaseName("ix_speaking_tasks_tenant_title_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(t => t.Title)
            .IsUnique()
            .HasDatabaseName("ix_speaking_tasks_global_title_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(t => t.DomainEvents);
    }
}
