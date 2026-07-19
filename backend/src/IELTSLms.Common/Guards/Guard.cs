using System.Runtime.CompilerServices;

namespace IELTSLms.Common.Guards;

/// <summary>
/// Fail-fast argument checks for defensive boundaries (public constructors, adapter inputs).
/// These throw for programmer errors; expected business-rule failures use result types instead
/// (Development Standards §10).
/// </summary>
public static class Guard
{
    /// <summary>Throws <see cref="ArgumentNullException"/> when <paramref name="value"/> is null.</summary>
    public static T AgainstNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>Throws when the string is null, empty, or whitespace.</summary>
    public static string AgainstNullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }

    /// <summary>Throws when the value is less than or equal to zero.</summary>
    public static int AgainstNonPositive(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0, paramName);
        return value;
    }

    /// <summary>Throws when the value is negative.</summary>
    public static int AgainstNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
        return value;
    }

    /// <summary>Throws when the GUID is <see cref="Guid.Empty"/>.</summary>
    public static Guid AgainstEmpty(Guid value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value must not be an empty GUID.", paramName);
        }

        return value;
    }
}
