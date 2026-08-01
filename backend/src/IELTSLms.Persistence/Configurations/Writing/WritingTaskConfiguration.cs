using IELTSLms.Domain.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Writing;

internal sealed class WritingTaskConfiguration : IEntityTypeConfiguration<WritingTask>
{
    public void Configure(EntityTypeBuilder<WritingTask> builder)
    {
        builder.ToTable("writing_tasks", t => t.HasCheckConstraint(
            "ck_writing_tasks_word_count_bounds", "\"max_word_count\" >= \"min_word_count\" AND \"min_word_count\" > 0"));

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => WritingTaskId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog task, visible read-only to every tenant.
        builder.Property(t => t.TenantId);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.TaskType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(t => t.Track).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(t => t.PromptText).HasColumnType("text").IsRequired();
        builder.Property(t => t.RubricReference).HasMaxLength(200);
        builder.Property(t => t.MinWordCount).IsRequired();
        builder.Property(t => t.MaxWordCount).IsRequired();
        builder.Property(t => t.CategoryId);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.DeletedAtUtc);

        builder.MapXminConcurrencyToken(t => t.Version);

        builder.HasIndex(t => t.TenantId).HasDatabaseName("ix_writing_tasks_tenant_id");
        builder.HasIndex(t => new { t.Track, t.TaskType }).HasDatabaseName("ix_writing_tasks_track_task_type");

        // Two partial indexes, same reasoning as ReadingPassage/ListeningAudio: Postgres treats every
        // NULL tenant_id as distinct, so a single (tenant_id, title) unique index would not constrain
        // global rows at all.
        builder.HasIndex(t => new { t.TenantId, t.Title })
            .IsUnique()
            .HasDatabaseName("ix_writing_tasks_tenant_title_unique")
            .HasFilter("\"tenant_id\" IS NOT NULL");

        builder.HasIndex(t => t.Title)
            .IsUnique()
            .HasDatabaseName("ix_writing_tasks_global_title_unique")
            .HasFilter("\"tenant_id\" IS NULL");

        builder.Ignore(t => t.DomainEvents);
    }
}
