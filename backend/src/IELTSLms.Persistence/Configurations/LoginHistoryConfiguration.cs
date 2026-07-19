using IELTSLms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Maps the <see cref="LoginHistory"/> append-only audit record (Auth §6). No concurrency token or
/// soft-delete: rows are written once and never mutated. The user id is nullable because a failed
/// attempt may not match any account.
/// </summary>
internal sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("login_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, value => LoginHistoryId.From(value))
            .ValueGeneratedNever();

        builder.Property(h => h.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? (UserId?)null : UserId.From(value.Value));

        builder.Property(h => h.EmailAttempted).HasMaxLength(Email.MaxLength).IsRequired();
        builder.Property(h => h.Outcome).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(h => h.IpAddress).HasMaxLength(64);
        builder.Property(h => h.UserAgent).HasMaxLength(512);
        builder.Property(h => h.OccurredAtUtc).IsRequired();

        // Query recent attempts per user and per attempted email for security review.
        builder.HasIndex(h => h.UserId).HasDatabaseName("ix_login_history_user_id");
        builder.HasIndex(h => new { h.EmailAttempted, h.OccurredAtUtc })
            .HasDatabaseName("ix_login_history_email_attempted_occurred_at_utc");

        builder.Ignore(h => h.DomainEvents);
    }
}
