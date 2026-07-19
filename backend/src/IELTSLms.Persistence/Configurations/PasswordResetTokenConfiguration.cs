using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="PasswordResetToken"/> aggregate: strongly-typed ids, the unique stored token
/// hash used for lookup, and single-use consumption columns (Auth §5).
/// </summary>
internal sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => PasswordResetTokenId.From(value))
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
            .HasDatabaseName("ix_password_reset_tokens_token_hash");

        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_password_reset_tokens_user_id");

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsConsumed);
    }
}
