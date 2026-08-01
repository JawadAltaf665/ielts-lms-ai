namespace IELTSLms.Common.Pagination;

/// <summary>A cursor-paginated page of results (API Design Standards §6.1) — for high-churn/volatile
/// collections where an offset page number isn't stable, unlike <see cref="PagedList{T}"/>.</summary>
public sealed class CursorPage<T>
{
    private CursorPage(IReadOnlyList<T> items, string? nextCursor)
    {
        Items = items;
        NextCursor = nextCursor;
    }

    public IReadOnlyList<T> Items { get; }

    /// <summary>Opaque cursor for the next page, or <see langword="null"/> when there's no more data.</summary>
    public string? NextCursor { get; }

    public bool HasMore => NextCursor is not null;

    public static CursorPage<T> Create(IReadOnlyList<T> items, string? nextCursor) => new(items, nextCursor);
}
