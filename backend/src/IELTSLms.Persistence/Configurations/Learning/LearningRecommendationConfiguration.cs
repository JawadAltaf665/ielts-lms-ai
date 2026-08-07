using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Learning;

internal sealed class LearningRecommendationConfiguration : IEntityTypeConfiguration<LearningRecommendation>
{
    public void Configure(EntityTypeBuilder<LearningRecommendation> builder)
    {
        builder.ToTable("learning_recommendations");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => LearningRecommendationId.From(value))
            .ValueGeneratedNever();

        builder.Property(r => r.UserId).IsRequired();

        // Structured JSON: [{ skill, title, rationale, priority }] — see RecommendationItemDto (Application).
        builder.Property(r => r.RankedItemsJson).HasColumnType("jsonb").IsRequired();

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.Property(r => r.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));

        builder.Property(r => r.GeneratedAtUtc).IsRequired();

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.UpdatedAtUtc);
        builder.Property(r => r.IsDeleted).IsRequired();
        builder.Property(r => r.DeletedAtUtc);

        builder.MapXminConcurrencyToken(r => r.Version);
        builder.MapTenantScope("learning_recommendations");

        // Partial unique index: at most one Active set per learner at a time (mirrors MockExam's
        // single-active-row pattern) — defense in depth alongside the application-level supersede step.
        builder.HasIndex(r => new { r.TenantId, r.UserId })
            .IsUnique()
            .HasDatabaseName("ix_learning_recommendations_tenant_user_active_unique")
            .HasFilter("\"status\" = 'Active'");

        builder.HasIndex(r => new { r.TenantId, r.UserId, r.GeneratedAtUtc })
            .HasDatabaseName("ix_learning_recommendations_tenant_user_generated");

        builder.Ignore(r => r.DomainEvents);
    }
}
