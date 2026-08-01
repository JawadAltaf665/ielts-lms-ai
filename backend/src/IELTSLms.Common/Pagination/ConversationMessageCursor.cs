using System.Globalization;
using System.Text;

namespace IELTSLms.Common.Pagination;

/// <summary>
/// Opaque cursor codec for a conversation's message-history page: the exclusive-upper-bound
/// <c>Sequence</c>, per API Design Standards §6.1. <c>Sequence</c> alone is a sufficient sort key —
/// unlike <see cref="ExamHistoryCursor"/>'s merged-source history, messages have one monotonic, unique
/// per-conversation ordering key, so no tie-breaker id is needed.
/// </summary>
public static class ConversationMessageCursor
{
    public static string Encode(int sequence) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(sequence.ToString(CultureInfo.InvariantCulture)));

    public static bool TryDecode(string? cursor, out int sequence)
    {
        sequence = default;

        if (string.IsNullOrEmpty(cursor))
        {
            return false;
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out sequence);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
