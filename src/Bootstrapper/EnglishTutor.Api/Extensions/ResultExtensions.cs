using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok(new ApiResponse(true, null, null));

        return MapError(result.Error);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(new ApiResponse<T>(true, result.Value, null));

        return MapError(result.Error);
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, string? location = null)
    {
        if (result.IsSuccess)
            return Results.Created(location, new ApiResponse<T>(true, result.Value, null));

        return MapError(result.Error);
    }

    private static IResult MapError(Error error)
    {
        var response = new ApiResponse(false, null, new ApiError(error.Code, error.Message, error.Details));

        return error.Code switch
        {
            "Error.NotFound" => Results.NotFound(response),
            "Error.Validation" => Results.UnprocessableEntity(response),
            "Error.Conflict" => Results.Conflict(response),
            "Error.Unauthorized" => Results.Json(response, statusCode: StatusCodes.Status401Unauthorized),
            "Error.Forbidden" => Results.Json(response, statusCode: StatusCodes.Status403Forbidden),
            _ => Results.Json(response, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}

public sealed record ApiResponse(bool IsSuccess, object? Data, ApiError? Error);
public sealed record ApiResponse<T>(bool IsSuccess, T? Data, ApiError? Error);
public sealed record ApiError(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null);
