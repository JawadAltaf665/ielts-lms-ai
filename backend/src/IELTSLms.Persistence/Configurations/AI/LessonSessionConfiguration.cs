using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class LessonSessionConfiguration : IEntityTypeConfiguration<LessonSession>
{
    public void Configure(EntityTypeBuilder<LessonSession> builder)
    {
        builder.ToTable("lesson_sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => LessonSessionId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.LessonId).IsRequired();
        builder.Property(s => s.ConversationId)
            .HasConversion(id => id.Value, value => ConversationId.From(value))
            .IsRequired();
        builder.Property(s => s.CurrentStep).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(s => s.CheckAttempts).IsRequired();
        builder.Property(s => s.StartedAtUtc).IsRequired();
        builder.Property(s => s.CompletedAtUtc);

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);
        builder.Property(s => s.IsDeleted).IsRequired();
        builder.Property(s => s.DeletedAtUtc);

        builder.MapXminConcurrencyToken(s => s.Version);
        builder.MapTenantScope("lesson_sessions");

        // One-active-session-per-user-per-lesson invariant (sprint business rule, mirrors
        // MonthlyMockExamConfiguration's single-active-session pattern): a partial unique index as the
        // DB backstop under a race, in addition to StartLessonSessionCommandHandler's pre-check.
        builder.HasIndex(s => new { s.TenantId, s.UserId, s.LessonId })
            .IsUnique()
            .HasFilter("\"current_step\" <> 'Completed'")
            .HasDatabaseName("ix_lesson_sessions_tenant_user_lesson_active");

        builder.HasIndex(s => s.ConversationId).HasDatabaseName("ix_lesson_sessions_conversation_id");

        builder.Ignore(s => s.DomainEvents);
    }
}
