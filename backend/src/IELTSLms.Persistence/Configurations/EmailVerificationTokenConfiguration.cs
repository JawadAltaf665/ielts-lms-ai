using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="EmailVerificationToken"/> aggregate: strongly-typed ids, the unique stored
/// token hash used for lookup, and single-use consumption columns (Auth §5).
/// </summary>
internal sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => EmailVerificationTokenId.From(value))
            .ValueGeneratedNever();

        builder.Property(t => t.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();

        builder.Property(t => t.ExpiresAtUtc).IsRequired();
        builder.Property(t => t.ConsumedAtUtc);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);

        builder.MapXminConcurrencyToken(t => t.Version);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_email_verification_tokens_token_hash");

        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_email_verification_tokens_user_id");

        builder.MapTenantScope("email_verification_tokens");

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsConsumed);
    }
}
