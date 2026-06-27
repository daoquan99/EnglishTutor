namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    ApiProblem? Error,
    ApiMetadata? Meta);

public sealed record ApiProblem(
    string Type,
    string Title,
    int Status,
    string Code,
    string Message,
    string TraceId,
    string CorrelationId,
    IReadOnlyDictionary<string, string[]>? ValidationErrors);

public sealed record ApiMetadata(PaginationMetadata? Pagination);

public sealed record PaginationMetadata(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
