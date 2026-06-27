namespace EnglishTutor.BuildingBlocks.Application.Pagination;

public record PagedResult<T>
{
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const int MinPage = 1;

    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    /// <summary>
    /// Page clamped to <c>[1, +∞)</c> so SQL OFFSET never receives a negative or
    /// zero value.
    /// </summary>
    public int SafePage => Math.Max(MinPage, Page);

    /// <summary>
    /// Page size clamped to <c>[1, 100]</c> — safe for division and for
    /// bounding result sets. Independent from <see cref="PaginationRequest.SafePageSize"/>;
    /// this guards against callers that bypass the request DTO and construct
    /// <see cref="PagedResult{T}"/> directly.
    /// </summary>
    public int SafePageSize => Math.Clamp(PageSize, MinPageSize, MaxPageSize);

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / SafePageSize);

    public bool HasPreviousPage => SafePage > 1;
    public bool HasNextPage => SafePage < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
