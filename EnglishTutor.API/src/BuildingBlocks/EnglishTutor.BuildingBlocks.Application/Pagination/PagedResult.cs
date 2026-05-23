namespace EnglishTutor.BuildingBlocks.Application.Pagination;

public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(Math.Max(0, TotalCount) / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
