using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IELTSLms.Persistence;

/// <summary>
/// Rewrites table, column, key, and index names to <c>snake_case</c> so the generated PostgreSQL
/// schema follows Postgres conventions and avoids quoted-identifier friction (Backend Architecture §10).
/// Applied last in <c>OnModelCreating</c> so it also normalizes names set by entity configurations.
/// </summary>
internal static class SnakeCaseNamingConvention
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName is not null)
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            var storeObject = StoreObjectIdentifier.Create(entity, StoreObjectType.Table);

            foreach (var property in entity.GetProperties())
            {
                var columnName = storeObject is { } target
                    ? property.GetColumnName(target)
                    : property.GetColumnName();

                if (columnName is not null)
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var name = key.GetName();
                if (name is not null)
                {
                    key.SetName(ToSnakeCase(name));
                }
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var name = foreignKey.GetConstraintName();
                if (name is not null)
                {
                    foreignKey.SetConstraintName(ToSnakeCase(name));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (name is not null)
                {
                    index.SetDatabaseName(ToSnakeCase(name));
                }
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder(input.Length + 8);

        for (var i = 0; i < input.Length; i++)
        {
            var current = input[i];

            if (char.IsUpper(current) && i > 0 && input[i - 1] != '_')
            {
                var previous = input[i - 1];
                var next = i + 1 < input.Length ? input[i + 1] : '\0';

                // Insert a break before an uppercase letter that starts a new word, i.e. when the
                // previous char is lowercase/digit, or when we're leaving an acronym run (ABCd -> ab_cd).
                if (!char.IsUpper(previous) || (char.IsUpper(previous) && char.IsLower(next)))
                {
                    builder.Append('_');
                }
            }

            builder.Append(char.ToLower(current, CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
