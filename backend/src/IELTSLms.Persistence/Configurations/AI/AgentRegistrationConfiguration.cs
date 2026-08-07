using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

/// <summary>
/// EF Core configuration for <see cref="AgentRegistration"/> (Sprint 10, Agent Registry).
/// Global reference data — not tenant-scoped; the global query filter in
/// <see cref="ApplicationDbContext"/> skips this entity because it does not implement
/// <c>ITenantScoped</c>. Soft-delete filter still applies via <c>IsDeleted</c>.
/// </summary>
internal sealed class AgentRegistrationConfiguration : IEntityTypeConfiguration<AgentRegistration>
{
    public void Configure(EntityTypeBuilder<AgentRegistration> builder)
    {
        builder.ToTable("agent_registrations");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => AgentRegistrationId.From(value))
            .ValueGeneratedNever();

        builder.Property(a => a.AgentKey)
            .HasMaxLength(AgentRegistration.AgentKeyMaxLength)
            .IsRequired();

        builder.Property(a => a.DisplayName)
            .HasMaxLength(AgentRegistration.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(a => a.CapabilitiesJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.PromptFeature)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(a => a.ToolsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.BudgetJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        // Audit fields
        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.UpdatedAtUtc);
        builder.Property(a => a.IsDeleted).IsRequired();
        builder.Property(a => a.DeletedAtUtc);

        builder.MapXminConcurrencyToken(a => a.Version);

        // AgentKey is the stable platform-wide slug — must be unique.
        builder.HasIndex(a => a.AgentKey)
            .IsUnique()
            .HasDatabaseName("uq_agent_registrations_agent_key");

        // Status index for the common "list all enabled agents" query.
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("ix_agent_registrations_status");

        builder.Ignore(a => a.DomainEvents);
    }
}
