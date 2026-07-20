namespace IELTSLms.Common.Pagination;

/// <summary>
/// Shared paging limits. List endpoints are paginated by default and cap page size to protect the
/// database and payload sizes (API Design Standards §6).
/// </summary>
public static class PaginationDefaults
{
    /// <summary>The first (1-based) page number.</summary>
    public const int FirstPage = 1;

    /// <summary>Page size used when the caller does not specify one.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Maximum page size a caller may request.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Clamps a requested page number to a valid (>= 1) value.</summary>
    public static int NormalizePage(int page) => page < FirstPage ? FirstPage : page;

    /// <summary>Clamps a requested page size into the [1, <see cref="MaxPageSize"/>] range.</summary>
    public static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => pageSize,
    };
}
