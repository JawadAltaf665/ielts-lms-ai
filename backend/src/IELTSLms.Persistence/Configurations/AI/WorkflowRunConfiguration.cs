using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

/// <summary>
/// EF Core configuration for <see cref="WorkflowRun"/> (Sprint 10, Workflow Engine).
/// Tenant-scoped and append-mostly — rows are created in Running state and updated once to a terminal
/// state. The global tenant + soft-delete query filters from <see cref="ApplicationDbContext"/> apply.
/// </summary>
internal sealed class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRun>
{
    public void Configure(EntityTypeBuilder<WorkflowRun> builder)
    {
        builder.ToTable("workflow_runs");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, value => WorkflowRunId.From(value))
            .ValueGeneratedNever();

        builder.Property(w => w.UserId).IsRequired();

        builder.Property(w => w.WorkflowKey)
            .HasMaxLength(WorkflowRun.WorkflowKeyMaxLength)
            .IsRequired();

        builder.Property(w => w.TriggerRef)
            .HasMaxLength(WorkflowRun.TriggerRefMaxLength)
            .IsRequired();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(w => w.StartedAt).IsRequired();
        builder.Property(w => w.CompletedAt);

        // StepTraceJson is a structured JSON blob — no max length, stored as text.
        builder.Property(w => w.StepTraceJson).HasColumnType("text");

        // Audit fields
        builder.Property(w => w.CreatedAtUtc).IsRequired();
        builder.Property(w => w.UpdatedAtUtc);
        builder.Property(w => w.IsDeleted).IsRequired();
        builder.Property(w => w.DeletedAtUtc);

        builder.MapXminConcurrencyToken(w => w.Version);
        builder.MapTenantScope("workflow_runs");

        // Primary query pattern: find all runs for a workflow key within a tenant.
        builder.HasIndex(w => new { w.TenantId, w.WorkflowKey })
            .HasDatabaseName("ix_workflow_runs_tenant_workflow_key");

        // Lookup by trigger (e.g. MockExamId → its workflow run).
        builder.HasIndex(w => new { w.TenantId, w.TriggerRef })
            .HasDatabaseName("ix_workflow_runs_tenant_trigger_ref");

        builder.Ignore(w => w.DomainEvents);
    }
}
