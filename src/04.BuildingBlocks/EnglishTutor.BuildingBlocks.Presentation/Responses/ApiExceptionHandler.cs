using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (exception is BadHttpRequestException)
        {
            _logger.LogWarning(
                "Rejected malformed HTTP request {TraceId}.",
                httpContext.TraceIdentifier);

            await ApiResults.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ApiErrorCodes.InvalidRequest,
                    title: "Invalid request",
                    message: "The request body or parameters are invalid.")
                .ExecuteAsync(httpContext);
            return true;
        }

        _logger.LogError(
            exception,
            "Unhandled API exception for trace {TraceId}.",
            httpContext.TraceIdentifier);

        await ApiResults.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                code: ApiErrorCodes.Unexpected,
                title: "Unexpected error",
                message: "An unexpected error occurred.")
            .ExecuteAsync(httpContext);
        return true;
    }
}
