namespace EnglishTutor.Api.Extensions;

public static class EndpointFilterExtensions
{
    public static RouteHandlerBuilder ProducesStandardErrors(this RouteHandlerBuilder builder) =>
        builder
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiResponse>(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}
