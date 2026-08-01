using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => ConversationId.From(value))
            .ValueGeneratedNever();

        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.Feature).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(c => c.StartedAtUtc).IsRequired();
        builder.Property(c => c.LastMessageAtUtc).IsRequired();

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.UpdatedAtUtc);
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.DeletedAtUtc);

        builder.MapXminConcurrencyToken(c => c.Version);
        builder.MapTenantScope("conversations");

        // A learner may have multiple conversations per feature — index, not a unique constraint.
        builder.HasIndex(c => new { c.TenantId, c.UserId, c.Feature }).HasDatabaseName("ix_conversations_tenant_user_feature");
        builder.HasIndex(c => new { c.TenantId, c.UserId, c.LastMessageAtUtc }).HasDatabaseName("ix_conversations_tenant_user_last_message_at");

        builder.Ignore(c => c.DomainEvents);
    }
}
