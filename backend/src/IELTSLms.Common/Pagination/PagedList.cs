namespace IELTSLms.Common.Pagination;

/// <summary>
/// A single page of results plus the metadata a client needs to render pagination
/// (API Design Standards §6). Construct via <see cref="Create"/> so counts stay consistent.
/// </summary>
/// <typeparam name="T">The item type on the page.</typeparam>
public sealed class PagedList<T>
{
    private PagedList(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>The 1-based page number.</summary>
    public int Page { get; }

    /// <summary>The page size used for this result.</summary>
    public int PageSize { get; }

    /// <summary>Total number of items across all pages.</summary>
    public long TotalCount { get; }

    /// <summary>Total number of pages given <see cref="PageSize"/>.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > PaginationDefaults.FirstPage;

    /// <summary>Whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Creates a page, normalizing the page number and size to safe bounds.</summary>
    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
    {
        var normalizedPage = PaginationDefaults.NormalizePage(page);
        var normalizedPageSize = PaginationDefaults.NormalizePageSize(pageSize);
        return new PagedList<T>(items, normalizedPage, normalizedPageSize, totalCount);
    }

    /// <summary>Creates an empty page for the given paging parameters.</summary>
    public static PagedList<T> Empty(int page, int pageSize) => Create([], page, pageSize, 0);
}
