using IELTSLms.Domain.Grammar;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Grammar;

internal sealed class GrammarProgressConfiguration : IEntityTypeConfiguration<GrammarProgress>
{
    public void Configure(EntityTypeBuilder<GrammarProgress> builder)
    {
        builder.ToTable("grammar_progress");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => GrammarProgressId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.GrammarTopicId)
            .HasConversion(id => id.Value, value => GrammarTopicId.From(value))
            .IsRequired();

        builder.Property(p => p.MasteryLevel).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(p => p.AttemptCount).IsRequired();
        builder.Property(p => p.LastAttemptedAtUtc);

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("grammar_progress");

        builder.HasIndex(p => new { p.TenantId, p.UserId, p.GrammarTopicId })
            .IsUnique()
            .HasDatabaseName("ix_grammar_progress_tenant_user_topic_unique");

        builder.Ignore(p => p.DomainEvents);
    }
}
