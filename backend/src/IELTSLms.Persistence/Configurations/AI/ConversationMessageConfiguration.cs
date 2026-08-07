using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => ConversationMessageId.From(value))
            .ValueGeneratedNever();

        builder.Property(m => m.ConversationId)
            .HasConversion(id => id.Value, value => ConversationId.From(value))
            .IsRequired();
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(m => m.Content).HasColumnType("text").IsRequired();
        builder.Property(m => m.Sequence).IsRequired();
        builder.Property(m => m.TokenCount).IsRequired();
        builder.Property(m => m.AiResponseId)
            .HasConversion(id => id!.Value.Value, value => AiResponseId.From(value));

        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.UpdatedAtUtc);
        builder.Property(m => m.IsDeleted).IsRequired();
        builder.Property(m => m.DeletedAtUtc);

        builder.MapXminConcurrencyToken(m => m.Version);
        builder.MapTenantScope("conversation_messages");

        // Leading (TenantId, ConversationId, Sequence) per the sprint's partition-candidate note
        // (mirrors ai_responses' tenant+time-leading index) — the natural ordering key for both context
        // assembly and message-history pagination, and unique since Sequence is assigned once per turn.
        builder.HasIndex(m => new { m.TenantId, m.ConversationId, m.Sequence })
            .IsUnique()
            .HasDatabaseName("ix_conversation_messages_tenant_conversation_sequence_unique");

        builder.Ignore(m => m.DomainEvents);
    }
}
