namespace EnglishTutor.BuildingBlocks.Application.Results;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public bool HasMore => Page * PageSize < Total;
}
