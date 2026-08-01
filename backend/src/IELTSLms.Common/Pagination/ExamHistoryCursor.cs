using System.Globalization;
using System.Text;

namespace IELTSLms.Common.Pagination;

/// <summary>
/// Opaque cursor codec for <c>GetExamHistoryQuery</c>: sort key (<c>CreatedAtUtc</c>) + tie-breaker
/// (<c>Id</c>), per API Design Standards §6.1. ponytail: base64-encoded, not HMAC-signed — acceptable
/// because it only ever reorders data the caller already has authenticated GET access to (their own
/// tenant-scoped exam history), so a hand-crafted cursor can't expose anything the caller couldn't see
/// anyway. Sign with Data Protection if this ever needs to resist tampering for its own sake.
/// </summary>
public static class ExamHistoryCursor
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
