using System.Globalization;
using System.Text;

namespace IELTSLms.Common.Pagination;

/// <summary>
/// Opaque cursor codec for <c>GetWritingReviewQueueQuery</c>: sort key (<c>CreatedAtUtc</c>) +
/// tie-breaker (<c>Id</c>), per API Design Standards §6.1. Same not-HMAC-signed rationale as
/// <see cref="SpeakingReviewCursor"/>: it only ever reorders data the caller (a teacher, tenant-scoped)
/// already has authenticated GET access to.
/// </summary>
public static class WritingReviewCursor
{
    public static string Encode(DateTimeOffset createdAtUtc, Guid id) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{createdAtUtc.UtcTicks}_{id}"));

    public static bool TryDecode(string? cursor, out DateTimeOffset createdAtUtc, out Guid id)
    {
        createdAtUtc = default;
        id = default;

        if (string.IsNullOrEmpty(cursor))
        {
            return false;
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('_', 2);
            if (parts.Length != 2
                || !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks)
                || !Guid.TryParse(parts[1], out var parsedId))
            {
                return false;
            }

            createdAtUtc = new DateTimeOffset(ticks, TimeSpan.Zero);
            id = parsedId;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
