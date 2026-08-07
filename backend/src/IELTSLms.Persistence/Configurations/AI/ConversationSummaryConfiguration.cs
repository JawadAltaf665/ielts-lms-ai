using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations.AI;

internal sealed class ConversationSummaryConfiguration : IEntityTypeConfiguration<ConversationSummary>
{
    public void Configure(EntityTypeBuilder<ConversationSummary> builder)
    {
        builder.ToTable("conversation_summaries");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => ConversationSummaryId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.ConversationId)
            .HasConversion(id => id.Value, value => ConversationId.From(value))
            .IsRequired();
        builder.Property(s => s.SummaryText).HasColumnType("text").IsRequired();
        builder.Property(s => s.CoversUpToSequence).IsRequired();

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc);
        builder.Property(s => s.IsDeleted).IsRequired();
        builder.Property(s => s.DeletedAtUtc);

        builder.MapXminConcurrencyToken(s => s.Version);
        builder.MapTenantScope("conversation_summaries");

        // Context assembly always reads the single latest summary for a conversation.
        builder.HasIndex(s => new { s.TenantId, s.ConversationId, s.CoversUpToSequence })
            .HasDatabaseName("ix_conversation_summaries_tenant_conversation_covers_up_to");

        builder.Ignore(s => s.DomainEvents);
    }
}
