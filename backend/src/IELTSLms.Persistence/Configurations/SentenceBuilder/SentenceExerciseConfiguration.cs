using IELTSLms.Domain.SentenceBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.SentenceBuilder;

internal sealed class SentenceExerciseConfiguration : IEntityTypeConfiguration<SentenceExercise>
{
    public void Configure(EntityTypeBuilder<SentenceExercise> builder)
    {
        builder.ToTable("sentence_exercises");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => SentenceExerciseId.From(value))
            .ValueGeneratedNever();

        // Nullable: null = global catalog exercise, visible read-only to every tenant.
        builder.Property(e => e.TenantId);
        builder.Property(e => e.Prompt).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.TargetPatternJson).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.Difficulty).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc);
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedAtUtc);

        builder.MapXminConcurrencyToken(e => e.Version);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_sentence_exercises_tenant_id");
        builder.HasIndex(e => e.Difficulty).HasDatabaseName("ix_sentence_exercises_difficulty");

        builder.Ignore(e => e.DomainEvents);
    }
}
