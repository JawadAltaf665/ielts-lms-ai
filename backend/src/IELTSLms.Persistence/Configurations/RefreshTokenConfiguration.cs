using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="RefreshToken"/> aggregate: strongly-typed ids, the unique stored token hash
/// used for lookup, rotation/revocation columns, and the session key that links a token family
/// (Auth §4.2, §4.3).
/// </summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => RefreshTokenId.From(value))
            .ValueGeneratedNever();

        builder.Property(t => t.UserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(t => t.SessionId)
            .HasConversion(id => id.Value, value => UserSessionId.From(value))
            .IsRequired();

        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();

        builder.Property(t => t.ExpiresAtUtc).IsRequired();
        builder.Property(t => t.UsedAtUtc);
        builder.Property(t => t.RevokedAtUtc);
        builder.Property(t => t.RevokedReason).HasMaxLength(256);

        builder.Property(t => t.ReplacedByTokenId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (RefreshTokenId?)null : RefreshTokenId.From(value.Value));

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.UpdatedAtUtc);

        builder.MapXminConcurrencyToken(t => t.Version);

        // Token hashes are looked up on every refresh, and must be unique.
        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash");

        // Family revocation loads every token for a session (Auth §4.3).
        builder.HasIndex(t => t.SessionId).HasDatabaseName("ix_refresh_tokens_session_id");

        builder.MapTenantScope("refresh_tokens");

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsUsed);
        builder.Ignore(t => t.IsRevoked);
    }
}
