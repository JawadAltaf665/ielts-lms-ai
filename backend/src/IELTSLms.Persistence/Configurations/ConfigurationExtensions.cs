using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IELTSLms.Persistence.Configurations;

/// <summary>
/// Shared mapping helpers so every identity entity treats optimistic concurrency identically
/// (Backend Architecture §9): the domain's <c>uint Version</c> is bound to PostgreSQL's <c>xmin</c>
/// system column, which the database maintains automatically on every update.
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Maps a <c>uint Version</c> property onto the PostgreSQL <c>xmin</c> system column as the
    /// concurrency token. No physical column is added; <c>xmin</c> already exists on every row.
    /// </summary>
    public static void MapXminConcurrencyToken<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, uint>> versionProperty)
        where TEntity : class
    {
        builder.Property(versionProperty)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
