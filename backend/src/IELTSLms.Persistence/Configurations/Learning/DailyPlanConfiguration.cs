using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.Learning;

internal sealed class DailyPlanConfiguration : IEntityTypeConfiguration<DailyPlan>
{
    public void Configure(EntityTypeBuilder<DailyPlan> builder)
    {
        builder.ToTable("daily_plans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => DailyPlanId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.PlanDate).IsRequired();
        builder.Property(p => p.LearningPathId);

        // Structured JSON: [{ id, skill, title, description, durationMinutes, isCompleted, completedAtUtc }]
        // — see DailyPlanItemDto (Application).
        builder.Property(p => p.PlanItemsJson).HasColumnType("jsonb").IsRequired();

        builder.Property(p => p.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));

        builder.Property(p => p.GeneratedAtUtc).IsRequired();

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.UpdatedAtUtc);
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.DeletedAtUtc);

        builder.MapXminConcurrencyToken(p => p.Version);
        builder.MapTenantScope("daily_plans");

        // ERD ENT-034 business rule: one active plan per user per day.
        builder.HasIndex(p => new { p.TenantId, p.UserId, p.PlanDate })
            .IsUnique()
            .HasDatabaseName("ix_daily_plans_tenant_user_date_unique");

        builder.Ignore(p => p.DomainEvents);
    }
}
