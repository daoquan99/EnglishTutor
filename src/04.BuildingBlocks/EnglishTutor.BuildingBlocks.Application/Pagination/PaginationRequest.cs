namespace EnglishTutor.BuildingBlocks.Application.Pagination;

public record PaginationRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;
    private const int MinPage = 1;
    private const int MinPageSize = 1;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "desc";

    /// <summary>
    /// Page clamped to a minimum of 1 (always positive — safe for SQL OFFSET).
    /// </summary>
    public int SafePage => Math.Max(MinPage, Page);

    /// <summary>
    /// Page size clamped to <c>[1, 100]</c> so it never divides by zero and never
    /// returns an unbounded result set.
    /// </summary>
    public int SafePageSize => Math.Clamp(PageSize, MinPageSize, MaxPageSize);
}
