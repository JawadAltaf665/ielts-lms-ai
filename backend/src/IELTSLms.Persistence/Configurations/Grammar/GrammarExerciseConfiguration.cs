using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Grammar;

internal sealed class GrammarExerciseConfiguration : IEntityTypeConfiguration<GrammarExercise>
{
    public void Configure(EntityTypeBuilder<GrammarExercise> builder)
    {
        builder.ToTable("grammar_exercises");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => GrammarExerciseId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId);
        builder.Property(e => e.GrammarTopicId)
            .HasConversion(id => id.Value, value => GrammarTopicId.From(value))
            .IsRequired();

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Mode).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(e => e.Order).IsRequired();

        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc);
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedAtUtc);

        builder.MapXminConcurrencyToken(e => e.Version);

        builder.HasIndex(e => e.TenantId).HasDatabaseName("ix_grammar_exercises_tenant_id");
        builder.HasIndex(e => e.GrammarTopicId).HasDatabaseName("ix_grammar_exercises_topic_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
