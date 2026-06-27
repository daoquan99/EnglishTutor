using EnglishTutor.BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public static class ApiResults
{
    public static IResult Ok<T>(T data) =>
        new ApiHttpResult<T>(
            statusCode: StatusCodes.Status200OK,
            data: data,
            error: null,
            meta: null);

    public static IResult Created<T>(string location, T data) =>
        new ApiHttpResult<T>(
            statusCode: StatusCodes.Status201Created,
            data: data,
            error: null,
            meta: null,
            location: location);

    public static IResult Empty() =>
        new ApiHttpResult<object?>(
            statusCode: StatusCodes.Status200OK,
            data: null,
            error: null,
            meta: null);

    public static IResult Paged<T>(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var pagination = new PaginationMetadata(
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPreviousPage: page > 1,
            HasNextPage: page < totalPages);

        return new ApiHttpResult<IReadOnlyList<T>>(
            statusCode: StatusCodes.Status200OK,
            data: items,
            error: null,
            meta: new ApiMetadata(Pagination: pagination));
    }

    public static IResult Problem(
        int statusCode,
        string code,
        string title,
        string message,
        IReadOnlyDictionary<string, string[]>? validationErrors = null,
        string type = "about:blank")
    {
        var problem = new ApiProblem(
            Type: type,
            Title: title,
            Status: statusCode,
            Code: code,
            Message: message,
            TraceId: string.Empty,
            CorrelationId: string.Empty,
            ValidationErrors: validationErrors);

        return new ApiHttpResult<object?>(
            statusCode: statusCode,
            data: null,
            error: problem,
            meta: null);
    }

    public static IResult Validation(
        string code,
        IReadOnlyDictionary<string, string[]> validationErrors,
        string message = "One or more validation errors occurred.") =>
        Problem(
            statusCode: StatusCodes.Status400BadRequest,
            code: code,
            title: "Validation failed",
            message: message,
            validationErrors: validationErrors);

    public static IResult FromError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status400BadRequest, "Bad request")
        };

        return Problem(
            statusCode: statusCode,
            code: error.Code,
            title: title,
            message: error.Message);
    }
}
